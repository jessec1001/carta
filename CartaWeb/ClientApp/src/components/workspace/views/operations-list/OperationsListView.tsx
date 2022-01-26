import classNames from "classnames";
import { FC, useMemo, useRef, useState } from "react";
import { ObjectFilter } from "library/search";
import { Loading, Text } from "components/text";
import { useViews, Views } from "components/views";
import { OperationIcon, WorkflowIcon } from "components/icons";
import { Column, Row } from "components/structure";
import { SearchboxInput } from "components/input";
import { useWorkspace } from "components/workspace";
import { useAPI } from "hooks";
import { Operation } from "library/api/operations";
import { ButtonDropdown, IconButton } from "components/buttons";
import { WorkflowEditorView } from "components/workspace/views";
import styles from "./OperationsListView.module.css";
import DatasetAddView from "../DatasetAddView";

/** A component that renders a single operation in the current workspace. */
const OperationsListItem: FC<{
  operation: Operation;
  onWorkflow?: () => void;
}> = ({ operation, onWorkflow = () => null }) => {
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

/** A view that displays the list of operations in the current workspace.e */
const OperationsListView: FC = () => {
  // We use these contexts to handle opening and closing views and managing data.
  const { rootId, actions: viewActions } = useViews();
  const elementRef = useRef<HTMLDivElement>(null);
  // const listRef = useRef<HTMLUListElement>(null);

  // We use a state variable to indicate which item of the operations list is currently selected.
  // We use a state variable to indicate whether the currently selected item is being renamed.
  // By selecting an operation, we indicate that we are preparing to rename the operation.
  // const [selected, setSelected] = useState<string | null>(null);
  // const [renaming, setRenaming] = useState<boolean>(false);
  // const [name, setName] = useState<string>("");

  // We setup a query to filter the operations.
  const [query, setQuery] = useState("");
  const operationsFilter = new ObjectFilter(query, {});

  // FIXME: Temporarily store information for the dropdown menu.
  // const [dropdownOpen, setDropdownOpen] = useState(false);
  // const [dialogOpen, setDialogOpen] = useState<null | "blank" | "data">(null);

  // We load the operations.
  const { operations } = useWorkspace();

  // TODO: Have a tab history system so when the active tab is closed, the previous tab is activated.
  // FIXME: We need to handle adding a new operation.
  const { dataAPI, operationsAPI, workflowsAPI } = useAPI();
  const handleAddOperation = async (type: "blank" | "data") => {
    switch (type) {
      case "blank":
        // TODO: Use a dialog to prompt for the name of the new workflow.

        // Create a blank workflow.
        const workflow = await workflowsAPI.createWorkflow({
          name: "New Workflow",
        });

        // Create an operation based on the workflow.
        const operation = await operationsAPI.createOperation(
          "workflow",
          workflow.id
        );

        // Add the operation to the workspace.
        operations.CRUD.add(operation);
        break;
      case "data":
        // TODO: Use a dialog to prompt for the name of the new workflow and the data to use.
        // TODO: Implement.
        viewActions.addElementToContainer(
          rootId,
          <DatasetAddView
            onPick={async (
              data: { source: string; resource: string }[],
              name: string
            ) => {
              // Create a new workflow.
              const workflow = await workflowsAPI.createWorkflow({
                name: name.length > 0 ? name : "New Workflow",
              });

              // Create a graph visualization operation.
              const visOperation = await operationsAPI.createOperation(
                "visualizeGraphPlot",
                null,
                { Name: "Network Visualization" }
              );
              await workflowsAPI.addWorkflowOperation(
                workflow.id,
                visOperation.id
              );

              // TODO: We need to make a combine graph operation and place it inbetween the data and visualization operations.

              // Create each of the data operations.
              for (let k = 0; k < data.length; k++) {
                const datum = data[k];
                const datumTemplate = await dataAPI.getOperation(
                  datum.source,
                  datum.resource
                );
                const datumOperation = await operationsAPI.createOperation(
                  datumTemplate.type,
                  datumTemplate.subtype,
                  datumTemplate.default
                );
                await workflowsAPI.addWorkflowOperation(
                  workflow.id,
                  datumOperation.id
                );
                await workflowsAPI.addWorkflowConnection(workflow.id, {
                  source: { field: "Graph", operation: datumOperation.id },
                  target: { field: "Graph", operation: visOperation.id },
                });
              }

              // Add an output operation for the output of the visualizer.
              const outputOperation = await operationsAPI.createOperation(
                "workflowOutput",
                null,
                { Name: "Visualization" }
              );
              await workflowsAPI.addWorkflowOperation(
                workflow.id,
                outputOperation.id
              );
              await workflowsAPI.addWorkflowConnection(workflow.id, {
                source: { field: "Plot", operation: visOperation.id },
                target: { field: "Value", operation: outputOperation.id },
              });

              const workflowOperation = await operationsAPI.createOperation(
                "workflow",
                workflow.id
              );
              operations.CRUD.add(workflowOperation);
            }}
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
    >
      {/* Display a searchbox for filtering the operations alongside a button to add more operations. */}
      <Row
        className="OperationsListView-Header"
        style={{
          alignItems: "center",
          padding: "0.5rem",
        }}
      >
        <Column>
          <SearchboxInput onChange={setQuery} value={query} clearable />
        </Column>
        &nbsp;
        <ButtonDropdown
          options={{ blank: "From Blank", data: "From Data" }}
          auto="blank"
          className={classNames(styles.workflowButton)}
          onPick={(value) => handleAddOperation(value as any)}
        >
          New
        </ButtonDropdown>
      </Row>

      {/* Check if the operations are still loading. */}
      {/* If so, display some loading text. */}
      {!operations.value && <Loading />}

      {/* Otherwise, display the list of operations. */}
      {operations.value && (
        <ul role="presentation">
          {operationsFilter.filter(operations.value).map((operation) => (
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
          ))}
        </ul>
      )}
    </Views.Container>
  );
};

export default OperationsListView;
