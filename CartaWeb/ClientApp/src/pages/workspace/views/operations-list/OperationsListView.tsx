import classNames from "classnames";
import { FC, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { ObjectFilter } from "library/search";
import { Loading, Text } from "components/text";
import { useViews, Views } from "components/views";
import { OperationIcon, WorkflowIcon } from "components/icons";
import { Column, Row } from "components/structure";
import { SearchboxInput } from "components/input";
import { useWorkspace } from "components/workspace";
import { useAPI, useRefresh } from "hooks";
import { Operation } from "library/api/operations";
import { ButtonDropdown, IconButton } from "components/buttons";
import { WorkflowEditorView } from "pages/workspace/views";
import styles from "./OperationsListView.module.css";
import { seconds } from "library/utility";
import { Workflow, WorkflowTemplate, WorkspaceOperation } from "library/api";
import OperationFromDataView from "../operation-from-data";
import OperationFromBlankView from "../operation-from-blank";

// TODO: Add help popups to every view.

/** Represents an operation that can be loading. */
interface LoadableOperation extends WorkspaceOperation {
  /** Whether the operation has finished loading yet. */
  loading: boolean;
}

/** A view-specific component that renders a single operation from the workspace. */
const OperationsListItem: FC<{
  operation: Operation;
  // selected: boolean;
  // onRename?: () => void;
  onWorkflow?: () => void;
}> = ({ operation, onWorkflow = () => null }) => {
  // TODO: Make sure that we have a loading symbol while reloading the particular operation after renaming.

  return (
    <li
      className={classNames("OperationListView-OperationsListItem")}
      style={{
        display: "flex",
        justifyContent: "space-between",
        cursor: "pointer",
      }}
    >
      <Text align="middle">
        <OperationIcon padded />
        {operation.name ?? <Text color="muted">(Unnamed)</Text>}
      </Text>
      <Text align="middle">
        {operation.type === "workflow" && (
          <span
            style={{
              margin: "0rem 0.5rem",
            }}
          >
            <IconButton title="Workflow" onClick={onWorkflow}>
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
  // const listRef = useRef<HTMLUListElement>(null);

  // // We store which item of the operations list is currently selected and whether it is being renamed.
  // const [selected, setSelected] = useState<string | null>(null);
  // const [renaming, setRenaming] = useState<boolean>(false);
  // const [name, setName] = useState<string>("");

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
  const [operationsPartial, operationsError, operationsRefresh] = useRefresh<
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

  // TODO: We can make the following code more concise if we actually have a template for a workflow
  //       before constructing the workflow itself.
  const handleCreateWorkflow = async (
    workflow: Partial<Workflow>,
    template: WorkflowTemplate
  ) => {
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
    operationsRefresh();
  };
  const handleSelectWorkflow = async (type: "blank" | "data") => {
    // TODO: Before making API requests, tenatively add the new workflow to the list of operations.
    switch (type) {
      case "blank":
        viewActions.addElementToContainer(
          rootId,
          <OperationFromBlankView
            onSubmit={async (name: string) =>
              handleCreateWorkflow(
                { name },
                await workflowsAPI.createBlankWorkflowTemplate()
              )
            }
          />,
          true
        );
        break;
      case "data":
        viewActions.addElementToContainer(
          rootId,
          <OperationFromDataView
            onSubmit={async (
              data: { source: string; resource: string }[],
              name: string
            ) =>
              handleCreateWorkflow(
                { name },
                await workflowsAPI.createDataWorkflowTemplate(data)
              )
            }
          />,
          true
        );
        break;
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
        <Column>
          <SearchboxInput onChange={setQuery} value={query} clearable />
        </Column>
        &nbsp;
        <ButtonDropdown
          options={{ blank: "From Blank", data: "From Data" }}
          auto="blank"
          className={classNames(styles.workflowButton)}
          onPick={(value) => handleSelectWorkflow(value as any)}
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
        <ul role="presentation">
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
                    onWorkflow={() => {
                      if (!operation.subtype) return;
                      viewActions.addElementToContainer(
                        rootId,
                        <WorkflowEditorView
                          operation={operation}
                          workflowId={operation.subtype}
                        />,
                        true
                      );
                    }}
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
