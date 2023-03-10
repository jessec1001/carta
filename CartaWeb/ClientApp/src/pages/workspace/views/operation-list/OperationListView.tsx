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
import { LogSeverity } from "library/logging";
import { seconds } from "library/utility";
import {
  ArrowDownIcon,
  ArrowUpIcon,
  OperationIcon,
  RefreshIcon,
  SortAlphabeticIcon,
  SortTimeIcon,
  WorkflowIcon,
} from "components/icons";
import { SearchboxInput, TextFieldInput } from "components/input";
import { Column, Row } from "components/structure";
import { Loading, Text } from "components/text";
import { useViews, Views } from "components/views";
import { useWorkspace } from "components/workspace";
import { Dropdown } from "components/dropdown";
import { Link } from "components/link";
import { useNotifications } from "components/notifications/Context";
import WorkflowEditorView from "../workflow-editor";
import OperationFromDataView from "../operation-from-data";
import styles from "./OperationListView.module.css";
import { Button, ButtonGroup, CloseButton } from "components/buttons";
import { Modal } from "components/modal";

// TODO: Add additional state to capture operations that are currently being created.
// TODO: Add help buttons that link to documentation for every view.

// These represent types used for loading the operations reliably with tracking information.
type CompleteOperation = WorkspaceOperation & Operation;
type LoadableOperation = { id: string; operation: CompleteOperation };

/** The props for the {@link CompleteOperation} component. */
interface OperationItemProps extends ComponentProps<"li"> {
  /** The operation item to display. */
  operation?: CompleteOperation;
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
  /** Called when the operation is deleted. */
  onDelete?: () => void;
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
  onDelete = () => {},
  onClick,
  className,
  children,
  ...props
}) => {
  // We use some state to represent deletion of the operation.
  const [deleteModalActive, setDeleteModalActive] = useState(false);
  const [deleted, setDeleted] = useState(false);

  return (
    <li
      className={classNames(
        styles.item,
        { [styles.selected]: selected },
        { [styles.deleted]: deleted },
        className
      )}
      {...props}
    >
      <Modal
        blur
        uninteractive
        active={deleteModalActive}
        onClose={() => setDeleteModalActive(false)}
        className={styles.modal}
      >
        <Text padding="bottom">
          Are you sure you want to delete the operation "{name}"?
        </Text>
        <ButtonGroup stretch>
          <Button
            color="error"
            onClick={() => {
              onDelete();
              setDeleted(true);
              setDeleteModalActive(false);
            }}
          >
            Delete
          </Button>
        </ButtonGroup>
      </Modal>
      <Text align="middle" {...{ onClick }} {...{ className: styles.itemBody }}>
        <OperationIcon padded />
        {renaming ? (
          <TextFieldInput
            className={styles.input}
            autoFocus
            autoSelect
            value={name}
            onChange={onNameChanged}
            placeholder={name}
          />
        ) : name.length > 0 ? (
          name
        ) : (
          <Text color="muted">(Unnamed)</Text>
        )}
      </Text>
      <Text align="middle">
        {operation && operation.type === "workflow" && (
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
        {/* <Link
          className={styles.itemButton}
          title="Jobs"
          to="#"
          ignore
          color="secondary"
          onClick={onOpenJobs}
        >
          <JobsIcon padded />
        </Link> */}
        {operation && (
          <CloseButton
            title="Delete"
            onClick={() => setDeleteModalActive(true)}
          />
        )}
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

  // We store a workflow template if an operation is in the process of being created.
  const [template, setTemplate] = useState<WorkflowTemplate | null>(null);

  // We setup a query to filter the operations.
  const [query, setQuery] = useState("");
  const operationsFilter = new ObjectFilter(query, {});

  // We use the notifications system to log errors that have occurred.
  const { logger } = useNotifications();

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

        //We need to make sure that we remove operations that are no longer in the workspace.
        for (let i = 0; i < operations.length; i++) {
          let found: boolean = false;
          for (let j = 0; j < operationsLoadable.length; j++) {
            if (operationsLoadable[j].id === operations[i].id) {
              found = true;
              break;
            }
          }
          if (!found) operations.splice(i);
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

  const createWorkflow = useCallback(
    async (template: WorkflowTemplate, name: string) => {
      let operation: Operation;
      try {
        // Create a new workflow.
        const createdWorkflow = await workflowsAPI.createWorkflowFromTemplate(
          { name: name },
          template
        );

        // Create an operation instance to the workflow.
        operation = await operationsAPI.createOperation(
          "workflow",
          createdWorkflow.id,
          undefined,
          name
        );
      } catch (error: any) {
        logger.log({
          source: "Operation List View",
          severity: LogSeverity.Error,
          title: "Operation Creation Error",
          message:
            "An error occurred while trying to create the new operation.",
          data: error,
        });
        return;
      }

      // Add the operation instance to the workspace.
      try {
        await workspaceAPI.addWorkspaceOperation(workspace.id, operation.id);
        await operationsRefresh();
      } catch (error: any) {
        logger.log({
          source: "Operation List View",
          severity: LogSeverity.Error,
          title: "Operation Addition Error",
          message:
            "An error occurred while trying to add the new operation to the workspace.",
          data: error,
        });
      }
    },
    [
      logger,
      operationsRefresh,
      workspace,
      operationsAPI,
      workflowsAPI,
      workspaceAPI,
    ]
  );
  const configureWorkflow = async (type: "blank" | "data") => {
    setRenamingOp("TEMPLATE");
    switch (type) {
      case "blank":
        setTemplate(await workflowsAPI.createBlankWorkflowTemplate());
        break;
      case "data":
        viewActions.addElementToContainer(
          rootId,
          <OperationFromDataView
            onSubmit={async (data: { source: string; resource: string }[]) =>
              setTemplate(await workflowsAPI.createDataWorkflowTemplate(data))
            }
          />,
          true
        );
        break;
    }
  };

  // We use these methods for modifying the list of operations.
  const defaultOperationName = "New Operation";
  const renameOperation = useCallback(async () => {
    // If the operation is a template, create it.
    if (renamingOp === "TEMPLATE" && template) {
      createWorkflow(template, name);
      setTemplate(null);
      return;
    }

    // Send a request to rename the operation.
    try {
      // TODO: Indicate that the operation is being renamed with a loading indicator.
      if (!renamingOp) return;
      await operationsAPI.updateOperation({ id: renamingOp, name: name });
      await operationsRefresh();
    } catch (error: any) {
      logger.log({
        source: "Operation List View",
        severity: LogSeverity.Error,
        title: "Operation Creation Error",
        message: "An error occurred while trying to create the new operation.",
        data: error,
      });
    }
  }, [
    createWorkflow,
    logger,
    name,
    operationsAPI,
    operationsRefresh,
    renamingOp,
    template,
  ]);
  const deleteOperation = useCallback(
    async (ids: string[]) => {
      const results = await Promise.allSettled(
        ids.map(async (id) => {
          await workspaceAPI.removeWorkspaceOperation(workspace.id, id);
          await operationsAPI.deleteOperation(id);
        })
      );
      for (const result of results) {
        if (result.status === "rejected") {
          logger.log({
            source: "Operation List View",
            severity: LogSeverity.Error,
            title: "Operation Deletion Error",
            message: "An error occurred while trying to delete an operation.",
            data: result.reason,
          });
        }
      }

      await operationsRefresh();
    },
    [logger, operationsAPI, operationsRefresh, workspace.id, workspaceAPI]
  );

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
  const history = viewActions.getHistory();
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
          if (!renamingOp) deleteOperation(selectedOp);
          break;
      }
    };

    // Setup and teardown.
    // These shortcuts should be disabled if this is not the active view.
    if (history.length > 0 && history[history.length - 1] === viewId) {
      window.addEventListener("keydown", handlePotentialKey);
      return () => window.removeEventListener("keydown", handlePotentialKey);
    }
  }, [
    openWorkflowView,
    deleteOperation,
    renameOperation,
    renamingOp,
    selectedOp,
    operations,
    history,
    viewId,
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
          <Dropdown.Toggler caret>New</Dropdown.Toggler>
          <Dropdown.Area>
            <Dropdown.Item onClick={() => configureWorkflow("blank")}>
              Blank Operation
            </Dropdown.Item>
            <Dropdown.Item onClick={() => configureWorkflow("data")}>
              Data Visualization Operation
            </Dropdown.Item>
          </Dropdown.Area>
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

      {/* Display an in-progress operation creation if necessary. */}
      {template && (
        <OperationItem
          selected={true}
          renaming={true}
          name={name}
          onNameChanged={setName}
        />
      )}

      {/* Otherwise, display the list of operations. */}
      {operations && !(operations instanceof Error) && (
        <ul role="presentation" ref={listRef}>
          {operationsFilter
            .filter(operations)
            .sort((x, y) => (sortDirection ? -1 : +1) * sorts[sort](x, y))
            .map(({ operation }, index) => {
              if (operation === undefined) {
                return <OperationLoadingItem key={`load:${index}`} />;
              } else if (operation instanceof Error) {
                return (
                  <OperationErrorItem
                    key={`error:${index}`}
                    error={operation}
                  />
                );
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
                    onDelete={() => deleteOperation([operation.id])}
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
