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
import { ObjectFilter } from "library/search";
import { Loading, Text } from "components/text";
import { useViews, Views } from "components/views";
import { OperationIcon, WorkflowIcon } from "components/icons";
import { Column, Row } from "components/structure";
import { SearchboxInput } from "components/input";
import { useWorkspace } from "components/workspace";
import { useAPI, useNestedAsync } from "hooks";
import { Operation } from "library/api/operations";
import { ButtonDropdown, IconButton } from "components/buttons";
import { WorkflowEditorView } from "pages/workspace/views";
import styles from "./OperationsListView.module.css";
import { seconds } from "library/utility";
import { Workflow, WorkflowTemplate, WorkspaceOperation } from "library/api";
import OperationFromDataView from "../operation-from-data";

// TODO: Add help popups to every view.

/** Represents an operation item that is loaded and displayed by the {@link OperationsListItem} component. */
type OperationItem = WorkspaceOperation & Operation;

/** A view-specific component that renders a single operation from the workspace. */
const OperationsListItem: FC<
  ComponentProps<"li"> & {
    operation: Operation;
    selected: boolean;
    name: string;
    onRename?: (name: string) => void;
    onOpenJobs?: () => void;
    onOpenWorkflow?: () => void;
  }
> = ({
  operation,
  selected,
  name,
  onRename = () => {},
  onOpenJobs = () => {},
  onOpenWorkflow = () => {},
  className,
  children,
  ...props
}) => {
  // TODO: Make sure that we have a loading symbol while reloading the particular operation after renaming.

  return (
    <li className={classNames(styles.item, className)} {...props}>
      <Text align="middle">
        <OperationIcon padded />
        {operation.name ?? <Text color="muted">(Unnamed)</Text>}
      </Text>
      <Text align="middle">
        {operation.type === "workflow" && (
          <span className={styles.itemButtons}>
            <IconButton title="Workflow" onClick={onOpenWorkflow}>
              <WorkflowIcon />
            </IconButton>
          </span>
        )}
      </Text>
    </li>
  );
};

/** A view that displays the list of operations in the current workspace. */
const OperationsListView: FC = () => {
  // We use these contexts to handle opening and closing views and managing data.
  const { viewId, rootId, actions: viewActions } = useViews();
  const elementRef = useRef<HTMLDivElement>(null);
  const listRef = useRef<HTMLUListElement>(null);

  // // We store which item of the operations list is currently selected and whether it is being renamed.
  const [selected, setSelected] = useState<string | null>(null);
  const [renaming, setRenaming] = useState<boolean>(false);
  const [name, setName] = useState<string>("");

  // We setup a query to filter the operations.
  const [query, setQuery] = useState("");
  const operationsFilter = new ObjectFilter(query, {});

  const [operations, setOperations] = useState<Operation[]>([]);

  // We use the workspace to get the list of operations contained within.
  const { operationsAPI, workflowsAPI, workspaceAPI } = useAPI();
  const { workspace } = useWorkspace();
  const operationsFetcher = useCallback(async () => {
    const operations = await workspaceAPI.getWorkspaceOperations(workspace.id);
    return operations.map((operation) => ({
      ...operation,
      loading: false,
    }));
  }, [workspace.id, workspaceAPI]);
  const [operationsPartial, operationsRefresh] = useNestedAsync<
    typeof operationsFetcher,
    LoadableOperation[]
  >(operationsFetcher, seconds(30));

  useEffect(() => {
    if (operationsPartial) {
      // Load all of the corresponding operations by identifier.
      for (const operationPartial of operationsPartial) {
        (async () => {
          const operation = await operationsAPI.getOperation(
            operationPartial.id,
            false
          );
          setOperations((operations) => {
            const operationIndex = operations.findIndex(
              (op) => op.id === operation.id
            );
            if (operationIndex >= 0) {
              const newOperations = [...operations];
              newOperations[operationIndex] = operation;
              return newOperations;
            } else {
              return [...operations, operation];
            }
          });
        })();
      }
    }
  }, [operationsAPI, operationsPartial]);

  // We use these methods to handle opening a new corresponding to an operation in the list.
  const handleOpenWorkflowView = useCallback(
    (operation: OperationItem) => {
      if (!operation.subtype) return;
      viewActions.addElementToContainer(
        rootId,
        <WorkflowEditorView
          operation={operation}
          workflowId={operation.subtype}
        />,
        true
      );
    },
    [rootId, viewActions]
  );
  const handleOpenJobsView = (operation: OperationItem) => {};

  // We use these methods for modifying the list of operations.
  const defaultOperationName = "New Operation";
  const renameOperation = useCallback(() => {}, []);
  const deleteOperation = useCallback(() => {}, []);
  const createWorkflow = async (
    workflow: Partial<Workflow>,
    template: WorkflowTemplate
  ) => {
    // TODO: Before making API requests, tenatively add the new workflow to the list of operations.
    //       This will require linking a temporary identifier to be replaced.

    // Create a new workflow.
    const createdWorkflow = await workflowsAPI.createWorkflowFromTemplate(
      workflow,
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
        createWorkflow(
          { name: defaultOperationName },
          await workflowsAPI.createBlankWorkflowTemplate()
        );
        break;
      case "data":
        viewActions.addElementToContainer(
          rootId,
          <OperationFromDataView
            onSubmit={async (data: { source: string; resource: string }[]) =>
              createWorkflow(
                { name: defaultOperationName },
                await workflowsAPI.createDataWorkflowTemplate(data)
              )
            }
          />,
          true
        );
        break;
    }
  };

  // Allow for the user to stop renaming and deselect an operation.
  useEffect(() => {
    if (listRef.current) {
      const handlePotentialOutsideClick = (event: MouseEvent) => {
        if (listRef.current?.contains(event.target as Element)) {
          if (renaming) {
            // Submit the new name.
            setRenaming(false);
            renameOperation();
          }
          setSelected(null);
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
  }, [renameOperation, renaming]);

  // Allow the user to use keyboard shortcuts to perform or cancel renaming and deleting.
  useEffect(() => {
    const handlePotentialKey = (event: KeyboardEvent) => {
      switch (event.code) {
        case "Enter":
          // If renaming, we submit the new name.
          if (renaming) {
            setRenaming(false);
            renameOperation();
          }
          // Otherwise, we open the workflow for the selected operation if applicable.
          else if (selected) {
            handleOpenWorkflowView();
          }
          break;
        case "Escape":
          // If renaming, we cancel the rename.
          // If an operation is selected, unselect it.
          if (renaming) setRenaming(false);
          else if (selected) setSelected(null);
          break;
        case "Delete":
          // If an operation is selected, delete it.
          if (!renaming) deleteOperation();
          break;
      }
    };

    // Setup and teardown.
    window.addEventListener("keydown", handlePotentialKey);
    return () => window.removeEventListener("keydown", handlePotentialKey);
  }, [
    handleOpenWorkflowView,
    deleteOperation,
    renameOperation,
    renaming,
    selected,
  ]);

  // Handles the logic of selecting a particular operation.
  const handleSelect = (operation: OperationItem, event: React.MouseEvent) => {
    if (event.detail === 1) {
      // This corresponds to a single click.
      if (selected === operation.id) {
        // If the operation is already selected, we start renamining.
        if (!renaming) {
          setRenaming(true);
          setName(operation.name ?? "");
        }
      } else {
        // We make sure to submit a new name if we are renaming.
        if (renaming) {
          setRenaming(false);
          renameOperation();
        }

        // If the operation is not selected, we select it.
        setSelected(operation.id);
      }
    } else if (event.detail === 2) {
      // This corresponds to a double click.
      // We open a workflow for the selected operation if applicable.
      handleOpenWorkflowView(operation);
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
        <Column>
          <SearchboxInput onChange={setQuery} value={query} clearable />
        </Column>
        &nbsp;
        <ButtonDropdown
          options={{ blank: "From Blank", data: "From Data" }}
          auto="blank"
          className={classNames(styles.workflowButton)}
          onPick={(value) => configureWorkflow(value as any)}
        >
          New
        </ButtonDropdown>
      </Row>

      {/* If the operations are not available yet, display why. */}
      {!operationsPartial && (
        <div className={styles.info}>
          {/* Check if the operations are still loading and display some loading text if so. */}
          {!operationsError && <Loading />}

          {/* Check if there was an error in loading the operations and display it if so. */}
          {operationsError && (
            <Text color="error">
              An error occurred while loading operations.&nbsp;
              {`(${operationsError.message})`}
            </Text>
          )}
        </div>
      )}

      {/* Otherwise, display the list of operations. */}
      {operationsPartial && (
        <ul role="presentation" ref={listRef}>
          {operationsFilter
            .filter(operationsPartial)
            .map((loadableOperation) => {
              // Get the operation associated with the operation identifier.
              const operation = operations.find(
                (operation) => operation.id === loadableOperation.id
              );

              if (!operation) return <Loading>Loading operation</Loading>;
              else {
                return (
                  <OperationsListItem
                    key={operation.id}
                    operation={operation}
                    selected={selected === operation.id}
                    name={
                      selected === operation.id
                        ? name
                        : operation.name ?? defaultOperationName
                    }
                    onClick={(event) => handleSelect(operation, event)}
                    onRename={setName}
                    onOpenWorkflow={() => handleOpenWorkflowView(operation)}
                    onOpenJobs={() => handleOpenJobsView(operation)}
                  />
                );
              }
            })}
        </ul>
      )}
    </Views.Container>
  );
};

export default OperationsListView;
