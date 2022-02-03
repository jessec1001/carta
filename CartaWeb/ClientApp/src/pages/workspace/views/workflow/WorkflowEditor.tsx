import { CheckboxInput } from "components/input";
import { Text } from "components/text";
import { FunctionComponent, useRef } from "react";
import { useWorkflow } from "./WorkflowContext";
import { useState } from "react";
import OperationGrid from "./OperationGrid";
import { useEffect } from "react";
import OperationMenu from "./OperationMenu";
import "./WorkflowEditor.css";

const WorkflowEditor: FunctionComponent = () => {
  const {
    input,
    workflow,
    operations,
    operationTypes,
    selected,
    select,
    addOperation,
    removeOperation,
    removeConnection,
    cancelConnect,
    executeWorkflow,
    autoUpdate,
    setAutoUpdate,
  } = useWorkflow();

  const ref = useRef<HTMLDivElement>(null);

  const [menuPosition, setMenuPosition] = useState<{
    x: number;
    y: number;
  } | null>(null);
  const [mousePosition, setMousePosition] = useState<{
    x: number;
    y: number;
  }>({ x: 0, y: 0 });

  useEffect(() => {
    const handleMouseMove = (event: MouseEvent) => {
      if (!ref.current) return;
      const rect = ref.current.getBoundingClientRect();

      setMousePosition({
        x: event.clientX - rect.left,
        y: event.clientY - rect.top,
      });
    };

    window.addEventListener("mousemove", handleMouseMove);
    return () => window.removeEventListener("mousemove", handleMouseMove);
  }, []);
  useEffect(() => {
    const handleKeyboardShortcut = (event: KeyboardEvent) => {
      if (event.code === "Space" && event.ctrlKey) {
        setMenuPosition(mousePosition);
      }
      if (event.code === "Delete") {
        selected.forEach((operation) => {
          removeOperation(operation.id);
          workflow?.connections.forEach((connection) => {
            if (
              connection.target.operation === operation.id ||
              connection.source.operation === operation.id
            )
              removeConnection(connection.id);
          });
        });
        select([]);
      }
    };

    window.addEventListener("keydown", handleKeyboardShortcut);
    return () => window.removeEventListener("keydown", handleKeyboardShortcut);
  }, [
    mousePosition,
    removeConnection,
    removeOperation,
    select,
    selected,
    workflow,
  ]);
  useEffect(() => {
    const handleMouseShortcut = (event: MouseEvent) => {
      if (event.button === 2) {
        // Right click was pressed so we cancel a pending connection or a selected connection.
        cancelConnect();
      }
    };

    window.addEventListener("mousedown", handleMouseShortcut);
    return () => window.removeEventListener("mousedown", handleMouseShortcut);
  }, [cancelConnect]);
  // TODO: Show pending connections.

  const handleCreateOperation = (
    type: { type: string; subtype: string | null } | null
  ) => {
    if (type) {
      addOperation(type.type, type.subtype);
    }
    setMenuPosition(null);
  };

  // If the workflow does exist, we need to draw the grid of operations.
  return (
    <div
      style={{
        position: "relative",
        width: "100%",
        height: "100%",
        overflow: "hidden",
      }}
      ref={ref}
      onClick={(event) => {
        if ((event.target as HTMLElement).id === "workflow-editor") select([]);
      }}
    >
      <div
        style={{
          width: "100%",
          height: "100%",
          display: "flex",
          flexDirection: "column",
        }}
      >
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            padding: "1rem",
            flexGrow: 0,
            flexShrink: 0,
            backgroundColor: "var(--color-fill-element)",
          }}
        >
          <div
            style={{
              display: "flex",
            }}
          >
            <span
              className="WorkflowEditor-execute"
              onClick={() => executeWorkflow(input)}
            >
              <Text>Execute</Text>
            </span>
            <span style={{ margin: "0rem 0.5rem" }}>
              <Text>Auto Update</Text>
            </span>
            <CheckboxInput value={autoUpdate} onChange={setAutoUpdate} />
          </div>
        </div>
        <OperationGrid
          workflow={workflow}
          operations={operations}
          operationTypes={operationTypes}
        />
      </div>
      {menuPosition && (
        <OperationMenu
          position={menuPosition}
          onSelect={handleCreateOperation}
        />
      )}
      <div
        id="mouse-pointer"
        style={{
          position: "absolute",
          left: mousePosition.x - 4,
          top: mousePosition.y - 4,
          width: 0,
          height: 0,
        }}
      />
    </div>
  );
};

export default WorkflowEditor;
