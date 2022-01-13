import classNames from "classnames";
import { FC, useMemo, useRef, useState } from "react";
import { WorkspaceOperation } from "library/api";
import { ObjectFilter } from "library/search";
import { Loading, Text } from "components/text";
import { useViews, Views } from "components/views";
import { CaretIcon, OperationIcon, WorkflowIcon } from "components/icons";
import { Column, Row } from "components/structure";
import { SearchboxInput } from "components/input";
import { useWorkspace } from "../WorkspaceContext";
import { useAPI } from "hooks";
import { Operation } from "library/api/operations";
import { IconButton } from "components/buttons";
import WorkflowEditorView from "./WorkflowEditorView";

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
  const { viewId, rootId, actions: viewActions } = useViews();
  const elementRef = useRef<HTMLDivElement>(null);
  const listRef = useRef<HTMLUListElement>(null);

  // We use a state variable to indicate which item of the operations list is currently selected.
  // We use a state variable to indicate whether the currently selected item is being renamed.
  // By selecting an operation, we indicate that we are preparing to rename the operation.
  const [selected, setSelected] = useState<string | null>(null);
  const [renaming, setRenaming] = useState<boolean>(false);
  const [name, setName] = useState<string>("");

  // We setup a query to filter the operations.
  const [query, setQuery] = useState("");
  const operationsFilter = new ObjectFilter(query, {});

  // FIXME: Temporarily store information for the dropdown menu.
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const [dialogOpen, setDialogOpen] = useState<null | "blank" | "data">(null);

  // We load the operations.
  const { workspace, operations } = useWorkspace();

  // FIXME: We need to handle adding a new operation.
  const { operationsAPI, workflowsAPI, workspaceAPI } = useAPI();
  const handleAddOperation = async (type: "blankWorkflow" | "dataWorkflow") => {
    switch (type) {
      case "blankWorkflow":
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
      case "dataWorkflow":
        // TODO: Use a dialog to prompt for the name of the new workflow and the data to use.
        // TODO: Implement.
        break;
    }
  };
  const handleCompleteBlankDialog = () => {
    setDialogOpen(null);
    handleAddOperation("blankWorkflow");
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
        <div>
          <div
            style={{
              display: "flex",
              position: "relative",
            }}
          >
            <button
              style={{
                display: "flex",
                alignItems: "center",
                outline: "none",
                border: "none",
                borderRadius: dropdownOpen
                  ? "4px 0px 0px 0px"
                  : "4px 0px 0px 4px",
                borderRight: "1px solid var(--color-stroke-hairline)",
                padding: "0.3rem",
                backgroundColor: "var(--color-fill-primary)",
                color: "var(--color-stroke-primary)",
              }}
              onClick={() => handleAddOperation("blankWorkflow")}
            >
              New
            </button>
            <button
              style={{
                fontSize: "0.6rem",
                display: "flex",
                alignItems: "center",
                outline: "none",
                border: "none",
                borderRadius: dropdownOpen
                  ? "0px 4px 0px 0px"
                  : "0px 4px 4px 0px",
                padding: "0.3rem",
                backgroundColor: "var(--color-fill-primary)",
                color: "var(--color-stroke-primary)",
              }}
              onClick={() => setDropdownOpen((open) => !open)}
            >
              <CaretIcon direction="down" />
            </button>
            {dropdownOpen && (
              <div
                style={{
                  position: "absolute",
                  top: "100%",
                  right: "0px",
                  width: "8em",
                  borderRadius: "4px 0px 4px 4px",
                  border: "1px solid var(--color-stroke-hairline)",
                  background: "white",
                  zIndex: 100,
                  boxShadow: "var(--shadow)",
                }}
              >
                <ul
                  style={{
                    listStyle: "none",
                    margin: "0px",
                    padding: "0px",
                    cursor: "pointer",
                  }}
                >
                  <li
                    style={{
                      listStyle: "none",
                      margin: "0px",
                      padding: "0px 0.25em",
                    }}
                    onClick={() => handleAddOperation("blankWorkflow")}
                  >
                    Blank Workflow
                  </li>
                  <li
                    style={{
                      listStyle: "none",
                      margin: "0px",
                      padding: "0px 0.25em",
                    }}
                    onClick={() => handleAddOperation("dataWorkflow")}
                  >
                    Data Workflow
                  </li>
                </ul>
              </div>
            )}
          </div>
        </div>
        {/* TODO: Add a dropdown button component for adding new views. */}
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
