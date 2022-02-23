import classNames from "classnames";
import React, {
  ComponentProps,
  FC,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { useAPI, useNestedAsync } from "hooks";
import { Operation, WorkflowTemplate, WorkspaceOperation } from "library/api";
import { ObjectFilter } from "library/search";
import { seconds } from "library/utility";
import {
  ArrowDownIcon,
  ArrowUpIcon,
  JobsIcon,
  OperationIcon,
  RefreshIcon,
  SortAlphabeticIcon,
  SortTimeIcon,
  WorkflowIcon,
} from "components/icons";
import { SearchboxInput } from "components/input";
import { Column, Row } from "components/structure";
import { Loading, Text } from "components/text";
import { useViews, Views } from "components/views";
import { useWorkspace } from "components/workspace";
import WorkflowEditorView from "../workflow-editor";
import OperationFromDataView from "../operation-from-data";
import {
  Dropdown,
  DropdownArea,
  DropdownItem,
  DropdownToggler,
} from "components/dropdown";
import { Link } from "components/link";
import styles from "./OperationListView.module.css";

// TODO: We need to allow renaming operations.
// TODO: When an operation is initially created, it should immediately be placed in the renaming state.
// TODO: We need to make sure that we remove operations that are no longer in the workspace.
// TODO: Display a modal to confirm deletion of operations.
// TODO: Add additional state to capture operations that are currently being created.
// TODO: Add help popups to every view.

// These represent types used for loading the operations reliably with tracking information.
type CompleteOperation = WorkspaceOperation & Operation;
type LoadableOperation = { id: string; operation: CompleteOperation };

/** The props for the {@link CompleteOperation} component. */
interface OperationItemProps extends ComponentProps<"li"> {
  /** The operation item to display. */
  operation: CompleteOperation;
  /** Whether the operation item is selected. */
  selected: boolean;
  /** Whether the operation item is being renamed. */
  renaming: boolean;
  /** The name to display for the operation item. */
  name: string;

  /** Called when the operation name is changed. */
  onNameChanged?: (name: string) => void;
  /** Called when the jobs button of the operation is clicked. */
  onOpenJobs?: () => void;
  /** Called when the workflow button of the operation is clicked. */
  onOpenWorkflow?: () => void;
}

/** A view-specific component that renders an operation from the workspace. */
const OperationItem: FC<OperationItemProps> = ({
  operation,
  selected,
  renaming,
  name,
  onNameChanged = () => {},
  onOpenJobs = () => {},
  onOpenWorkflow = () => {},
  className,
  children,
  ...props
}) => {
  return (
    <li
      className={classNames(
        styles.item,
        { [styles.selected]: selected },
        className
      )}
      {...props}
    >
      <Text align="middle">
        <OperationIcon padded />
        {operation.name ?? <Text color="muted">(Unnamed)</Text>}
      </Text>
      <Text align="middle">
        {operation.type === "workflow" && (
          <Link
            className={styles.itemButton}
            title="Workflow"
            to="#"
            ignore
            color="secondary"
            onClick={onOpenWorkflow}
          >
            <WorkflowIcon padded />
          </Link>
        )}
        <Link
          className={styles.itemButton}
          title="Jobs"
          to="#"
          ignore
          color="secondary"
          onClick={onOpenJobs}
        >
          <JobsIcon padded />
        </Link>
      </Text>
    </li>
  );
};
/** A view-specific component that renders an error for an operation from the workspace. */
const OperationErrorItem: FC<{ error: Error }> = ({ error: { message } }) => {
  return (
    <li className={classNames(styles.item)}>
      <Text color="error">{message}</Text>
    </li>
  );
};
/** A view-specific component that renders a loading symbol for an operation from the workspace. */
const OperationLoadingItem: FC = () => {
  return (
    <li className={classNames(styles.item)}>
      <Loading />
    </li>
  );
};

/** A view that displays the list of operations in the current workspace. */
const OperationListView: FC = () => {
  // We use these contexts to handle opening and closing views and managing data.
  const { viewId, rootId, actions: viewActions } = useViews();
  const elementRef = useRef<HTMLDivElement>(null);
  const listRef = useRef<HTMLUListElement>(null);

  // We store which items of the operations list are currently selected and whether it is being renamed.
  const [selectMultiple, setSelectMultiple] = useState<boolean>(false);
  const [selectedOp, setSelectedOp] = useState<string[]>([]);
  const [renamingOp, setRenamingOp] = useState<string | null>(null);
  const [name, setName] = useState<string>("");

  // We setup a query to filter the operations.
  const [query, setQuery] = useState("");
  const operationsFilter = new ObjectFilter(query, {});

  // We use the workspace to get the list of operations contained within.
  const { operationsAPI, workflowsAPI, workspaceAPI } = useAPI();
  const { workspace } = useWorkspace();
  const operationsFetcher = useCallback(async () => {
    const operations = await workspaceAPI.getWorkspaceOperations(workspace.id);
    return operations.map((operation) => ({
      id: operation.id,
      operation: async () =>
        ({
          ...operation,
          ...(await operationsAPI.getOperation(operation.id)),
        } as CompleteOperation),
    }));
  }, [operationsAPI, workspaceAPI, workspace.id]);
  const [operationsLoadable, operationsRefresh] = useNestedAsync<
    typeof operationsFetcher,
    LoadableOperation[]
  >(operationsFetcher, false, seconds(30));
  const [operations, setOperations] =
    useState<typeof operationsLoadable>(operationsLoadable);

  // Whenever we load the operations, we need to update the list of operations.
  // We do this by buffering between loaded and loading states.
  useEffect(() => {
    // TODO: We need to make sure that we remove operations that are no longer in the workspace.
    if (
      !(operationsLoadable === undefined || operationsLoadable instanceof Error)
    ) {
      setOperations((operations) => {
        // If we are initially loading, the buffers should be equivalent.
        if (operations === undefined || operations instanceof Error)
          return operationsLoadable;

        // We avoid setting operations that were loaded previously but are being refreshed.
        for (const operationLoadable of operationsLoadable) {
          const index = operations.findIndex(
            (op) => op.id === operationLoadable.id
          );
          if (index === -1) operations = [...operations, operationLoadable];
          else if (operationLoadable.operation) {
            operations = [
              ...operations.slice(0, index),
              operationLoadable,
              ...operations.slice(index + 1),
            ];
          }
        }
        return operations;
      });
    }
  }, [operationsLoadable]);
  const refreshOperations = useCallback(() => {
    setOperations(undefined);
    operationsRefresh();
  }, [operationsRefresh]);

  // We use one of various sorting types to sort the operations.
  type ItemType = {
    id: string;
    operation: Error | CompleteOperation | undefined;
  };
  type SortType = "alphabetical" | "chronological";
  const sorts: Record<SortType, (op1: ItemType, op2: ItemType) => number> = {
    alphabetical: ({ operation: op1 }, { operation: op2 }) => {
      if (op1 instanceof Error && op2 === undefined) return -1;
      if (op2 instanceof Error && op1 === undefined) return +1;
      if (op1 instanceof Error || op1?.name === undefined) return +1;
      if (op2 instanceof Error || op2?.name === undefined) return -1;
      return op1.name.localeCompare(op2.name);
    },
    chronological: ({ operation: op1 }, { operation: op2 }) => {
      if (op1 instanceof Error && op2 === undefined) return -1;
      if (op2 instanceof Error && op1 === undefined) return +1;
      if (op1 instanceof Error || op1?.documentHistory?.dateAdded === undefined)
        return +1;
      if (op2 instanceof Error || op2?.documentHistory?.dateAdded === undefined)
        return -1;
      return +op2.documentHistory.dateAdded - +op1.documentHistory.dateAdded;
    },
  };
  const [sort, setSort] = useState<SortType>("chronological");
  const [sortDirection, setSortDirection] = useState<boolean>(false);
  const cycleSort = () => {
    const sortsKeys: SortType[] = ["alphabetical", "chronological"];
    setSort((sort) => {
      const prevIndex = sortsKeys.indexOf(sort);
      const nextIndex = (prevIndex + 1) % sortsKeys.length;
      return sortsKeys[nextIndex];
    });
  };

  // We use these methods to handle opening a new corresponding to an operation in the list.
  const openWorkflowView = useCallback(
    (operation: CompleteOperation) => {
      // TODO: Refactor the workflow editor view to use an operation ID, workflow ID, and optional job ID.
      if (!operation.subtype) return;
      viewActions.addElementToContainer(
        rootId,
        <WorkflowEditorView
          operationId={operation.id}
          workflowId={operation.subtype}
        />,
        true
      );
    },
    [rootId, viewActions]
  );
  const openJobsView = useCallback((operation: CompleteOperation) => {
    // TODO: Implement jobs view.
  }, []);

  // We use these methods for modifying the list of operations.
  const defaultOperationName = "New Operation";
  const renameOperation = useCallback(() => {
    // TODO: Implement.
  }, []);
  const deleteOperation = useCallback(() => {
    // TODO: Implement.
  }, []);
  const createWorkflow = async (template: WorkflowTemplate) => {
    // TODO: Before making API requests, tenatively add the new workflow to the list of operations.
    //       This will require linking a temporary identifier to be replaced.

    // TODO: Capture errors from the API and report to the logging system.

    // Create a new workflow.
    const createdWorkflow = await workflowsAPI.createWorkflowFromTemplate(
      { name: defaultOperationName },
      template
    );

    // Create an operation instance to the workflow.
    const operation = await operationsAPI.createOperation(
      "workflow",
      createdWorkflow.id
    );

    // Add the operation instance to the workspace.
    await workspaceAPI.addWorkspaceOperation(workspace.id, operation.id);
    await operationsRefresh();

    // TODO: Autofocus and start renaming the newly created operation.
  };
  const configureWorkflow = async (type: "blank" | "data") => {
    switch (type) {
      case "blank":
        createWorkflow(await workflowsAPI.createBlankWorkflowTemplate());
        break;
      case "data":
        viewActions.addElementToContainer(
          rootId,
          <OperationFromDataView
            onSubmit={async (data: { source: string; resource: string }[]) =>
              createWorkflow(
                await workflowsAPI.createDataWorkflowTemplate(data)
              )
            }
          />,
          true
        );
        break;
    }
  };

  // Allow for the user to stop renaming and/or deselect operations.
  useEffect(() => {
    if (listRef.current) {
      const handlePotentialOutsideClick = (event: MouseEvent) => {
        if (!listRef.current?.contains(event.target as Element)) {
          if (renamingOp) {
            // Submit the new name.
            setRenamingOp(null);
            renameOperation();
          }
          setSelectedOp([]);
        }
      };

      // Setup and teardown.
      if (elementRef.current) {
        const element = elementRef.current;
        element.addEventListener("click", handlePotentialOutsideClick);
        return () =>
          element.removeEventListener("click", handlePotentialOutsideClick);
      }
    }
  }, [renameOperation, renamingOp]);

  // Check if the control or command key is held down to indicate whether multiple items can be selected.
  useEffect(() => {
    const handleKeydown = (event: KeyboardEvent) => {
      if (event.metaKey || event.ctrlKey) setSelectMultiple(true);
    };
    const handleKeyup = (event: KeyboardEvent) => {
      if (!event.metaKey && !event.ctrlKey) setSelectMultiple(false);
    };

    // Setup and teardown.
    window.addEventListener("keydown", handleKeydown);
    window.addEventListener("keyup", handleKeyup);
    return () => {
      window.removeEventListener("keydown", handleKeydown);
      window.removeEventListener("keyup", handleKeyup);
    };
  }, []);

  // Allow the user to use keyboard shortcuts to perform or cancel renaming and deleting.
  useEffect(() => {
    const handlePotentialKey = (event: KeyboardEvent) => {
      switch (event.code) {
        case "Enter":
          if (renamingOp) {
            // If renaming, we submit the new name.
            setRenamingOp(null);
            renameOperation();
          } else {
            // Otherwise, we open the workflows for the selected operations if applicable.
            for (const operationId of selectedOp) {
              if (operations === undefined || operations instanceof Error)
                return;
              const operation = operations.find(
                (op) => op.id === operationId
              )?.operation;
              if (!(operation === undefined || operation instanceof Error))
                openWorkflowView(operation);
            }
          }
          break;
        case "Escape":
          // If renaming, we cancel the rename.
          // If an operation is selected, unselect it.
          if (renamingOp) setRenamingOp(null);
          else setSelectedOp([]);
          break;
        case "Delete":
          // If an operation is selected, delete it.
          if (!renamingOp) deleteOperation();
          break;
      }
    };

    // Setup and teardown.
    window.addEventListener("keydown", handlePotentialKey);
    return () => window.removeEventListener("keydown", handlePotentialKey);
  }, [
    openWorkflowView,
    deleteOperation,
    renameOperation,
    renamingOp,
    selectedOp,
    operations,
  ]);

  // The following is how keyboard and mouse events should be handled.
  // - Single click (once) only selects operations.
  // - Single click (twice) starts renaming the now one and only selected operation.
  // - Double click opens the workflow for the now one and only selected operation.
  // - Single control+click selects unselected operations and deselects selected operations.
  // - Double control+click opens the workflows for the selected operations.

  // Handles the logic of selecting a particular operation.
  const handleClickOperation = (
    operation: CompleteOperation,
    event: React.MouseEvent
  ) => {
    if (event.detail === 1) {
      // This corresponds to a single click.
      if (selectMultiple) {
        // We perform selection or deselection of the operation.
        setSelectedOp((selectedOp) => {
          if (selectedOp.includes(operation.id))
            return selectedOp.filter((op) => op !== operation.id);
          else return [...selectedOp, operation.id];
        });
      } else {
        if (selectedOp.includes(operation.id)) {
          // If the operation is already selected, we start renaming it.
          if (renamingOp === operation.id) return;
          else if (renamingOp) renameOperation();

          setRenamingOp(operation.id);
          setName(operation.name ?? "");
        } else {
          // If the operation is not selected, we select it.
          setSelectedOp([operation.id]);
        }
      }
    } else if (event.detail === 2) {
      // This corresponds to a double click.
      // We open workflows for the selected operations if applicable.
      for (const operationId of selectedOp) {
        if (operations === undefined || operations instanceof Error) return;
        const operation = operations.find(
          (op) => op.id === operationId
        )?.operation;
        if (!(operation === undefined || operation instanceof Error))
          openWorkflowView(operation);
      }
    }
  };

  // Create the view title component.
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <OperationIcon padded /> Operations
      </Text>
    );
  }, []);

  // Render the view component.
  return (
    <Views.Container
      title={title}
      closeable
      direction="vertical"
      ref={elementRef}
      onClick={() => viewActions.addHistory(viewId)}
    >
      {/* TODO: Replace split button dropdown with a simple dropdown. */}
      {/* Display a searchbox for filtering the operations alongside a button to add more operations. */}
      <Row className={styles.header}>
        <span
          className={styles.headerButton}
          onClick={() => refreshOperations()}
        >
          <RefreshIcon padded />
        </span>
        <Column>
          <SearchboxInput onChange={setQuery} value={query} clearable />
        </Column>
        &nbsp;
        <span className={styles.headerButton} onClick={() => cycleSort()}>
          {sort === "alphabetical" && <SortAlphabeticIcon />}
          {sort === "chronological" && <SortTimeIcon />}
        </span>
        <span
          className={styles.headerButton}
          onClick={() => setSortDirection((direction) => !direction)}
        >
          {sortDirection ? <ArrowUpIcon /> : <ArrowDownIcon />}
        </span>
        <Dropdown side="bottom-left">
          <DropdownToggler caret>New</DropdownToggler>
          <DropdownArea>
            <DropdownItem onClick={() => configureWorkflow("blank")}>
              Blank Operation
            </DropdownItem>
            <DropdownItem onClick={() => configureWorkflow("data")}>
              Data Visualization Operation
            </DropdownItem>
          </DropdownArea>
        </Dropdown>
      </Row>

      {/* If the operations are not available yet, display why. */}
      {(operations === undefined || operations instanceof Error) && (
        <div className={styles.info}>
          {/* Check if the operations are still loading and display some loading text if so. */}
          {operations === undefined && <Loading />}

          {/* Check if there was an error in loading the operations and display it if so. */}
          {operations instanceof Error && (
            <Text color="error">
              An error occurred while loading operations.&nbsp;
              {`(${operations.message})`}
            </Text>
          )}
        </div>
      )}

      {/* Otherwise, display the list of operations. */}
      {operations && !(operations instanceof Error) && (
        <ul role="presentation" ref={listRef}>
          {operationsFilter
            .filter(operations)
            .sort((x, y) => (sortDirection ? -1 : +1) * sorts[sort](x, y))
            .map(({ operation }) => {
              if (operation === undefined) {
                return <OperationLoadingItem />;
              } else if (operation instanceof Error) {
                return <OperationErrorItem error={operation} />;
              } else {
                return (
                  <OperationItem
                    key={operation.id}
                    operation={operation}
                    selected={selectedOp.includes(operation.id)}
                    renaming={renamingOp === operation.id}
                    name={
                      renamingOp === operation.id
                        ? name
                        : operation.name ?? defaultOperationName
                    }
                    onClick={(event) => handleClickOperation(operation, event)}
                    onNameChanged={setName}
                    onOpenWorkflow={() => openWorkflowView(operation)}
                    onOpenJobs={() => openJobsView(operation)}
                  />
                );
              }
            })}
        </ul>
      )}
    </Views.Container>
  );
};

export default OperationListView;
