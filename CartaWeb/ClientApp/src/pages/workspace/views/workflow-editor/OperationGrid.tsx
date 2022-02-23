import { Operation, OperationType, Workflow } from "library/api";
import React, { createContext, useEffect, useState } from "react";
import { useCallback } from "react";
import { FunctionComponent } from "react";
import { Arrows } from "components/arrows";
import OperationNode from "./OperationNode";
import { useWorkflow } from "./WorkflowContext";

interface OperationGridProps {
  workflow: Workflow;
  operations: Operation[];
  operationTypes: OperationType[];
}

const OperationGridContext = createContext<
  | {
      setVerticalPosition: (operationId: string, y: number) => void;
    }
  | undefined
>(undefined);

const OperationGrid: FunctionComponent<OperationGridProps> = ({ workflow }) => {
  const {
    connecting,
    operations,
    operationTypes,
    removeConnection,
    input,
    output,
  } = useWorkflow();
  const { connections } = workflow;
  // const updateXarrow = useXarrow();

  // Columns indicate chunk intervals per operation.
  // Rows indicate the next available screen position.
  const [columns, setColumns] = useState<Record<string, number>>({});
  const [rows, setRows] = useState<Record<string, number>>({});

  // TODO: Setup rows.
  useEffect(() => {
    let layoutChanging = true;
    while (layoutChanging) {
      layoutChanging = false;
      // eslint-disable-next-line no-loop-func
      operations.forEach((operation, index) => {
        setColumns((columns) => {
          // Initialize column for new operations.
          if (!(operation.id in columns)) {
            columns = { ...columns, [operation.id]: 0 };
            layoutChanging = true;
          }
          // Make sure that every operation is as far forward as necessary.
          connections.forEach((connection) => {
            if (connection.target.operation === operation.id) {
              const column = Math.max(
                columns[operation.id],
                (columns[connection.source.operation] ?? 0) + 1
              );
              if (columns[operation.id] !== column) {
                layoutChanging = true;
                columns = {
                  ...columns,
                  [operation.id]: column,
                };

                setRows((rows) => {
                  // Set rows here as well.
                  let childIndex = -1;
                  for (let k = 0; k < operations.length; k++) {
                    if (
                      columns[operations[k].id] === column &&
                      operations[k].id !== operation.id
                    )
                      childIndex = k;
                  }
                  const el = document.getElementById("workflow-editor")
                    ?.childNodes[childIndex] as HTMLDivElement | null;
                  const rowPosition = el
                    ? el.offsetTop + el.getBoundingClientRect().height + 16
                    : 16;

                  return { ...rows, [operation.id]: rowPosition };
                });
              }
            }
          });
          return columns;
        });
      });
      // eslint-disable-next-line no-loop-func
      operations.forEach((operation, index) => {
        setRows((rows) => {
          if (!(operation.id in rows)) {
            layoutChanging = true;
            const col = columns[operation.id] ?? 0;
            let childIndex = -1;
            for (let k = 0; k < operations.length; k++) {
              if (columns[operations[k].id] === col) childIndex = k;
            }
            const el = document.getElementById("workflow-editor")?.childNodes[
              childIndex
            ] as HTMLDivElement;
            const rowPosition = el
              ? el.offsetTop + el.getBoundingClientRect().height + 16
              : 16;
            // const rowPosition =
            // (Object.values(columns).filter((value) => {
            // return value === col;
            // }).length +
            // 1) *
            // 40;
            rows = { ...rows, [operation.id]: rowPosition };
          }
          return rows;
        });
      });
    }

    // Update columns (propagation) for updated connections.
  }, [columns, connections, operations]);

  // useEffect(() => {
  //   updateXarrow();
  // }, [columns]);

  const setVerticalPosition = useCallback((operationId: string, y: number) => {
    setRows((rows) => ({ ...rows, [operationId]: y }));
  }, []);

  return (
    <OperationGridContext.Provider
      value={{
        setVerticalPosition,
      }}
    >
      <div
        style={{
          position: "relative",
          display: "flex",
          flexGrow: 1,
          overflowX: "auto",
          padding: "2rem",
        }}
        id="workflow-editor"
      >
        <Arrows>
          {workflow.operations.map((operationId) => {
            const operation = operations.find(
              (operation) => operation.id === operationId
            );
            if (!operation) return null;

            const operationType = operationTypes.find(
              (type) =>
                type.type === operation.type &&
                type.subtype === operation.subtype
            );
            if (!operationType) return null;
            const nameProperty = operation.default
              ? operation.default["name"] ?? operation.default["Name"]
              : undefined;
            let data: any = undefined;
            if (nameProperty) {
              data = data ?? output[nameProperty];
              data = data ?? input[nameProperty];
            }

            // let columnIndex = 0;
            // for (let k = 0; k < operations.length; k++) {
            //   if (operations[k].id === operation.id) {
            //     break;
            //   }
            //   if (columns[operations[k].id] === columns[operation.id]) {
            //     columnIndex++;
            //   }
            // }

            return (
              <div
                key={operation.id}
                style={{
                  position: "absolute",
                  left: `${20 * columns[operation.id] + 2}rem`,
                  top: `${rows[operation.id]}px`,
                }}
              >
                <OperationNode
                  operation={operation}
                  type={operationType}
                  data={data}
                />
              </div>
            );
          })}
          {workflow.connections.map((connection) => {
            return (
              <Arrows.Arrow
                key={connection.id}
                source={`${connection.source.operation}:${connection.source.field}`}
                target={`${connection.target.operation}:${connection.target.field}`}
                onClick={(event: React.MouseEvent) => {
                  removeConnection(connection.id);
                  event.preventDefault();
                }}
                style={{
                  cursor: "pointer",
                }}
                // dashness={connection.multiplex}
                // startAnchor="right"
                // endAnchor="left"
                // strokeWidth={2}
                // color="var(--color-stroke-normal)"
                // curveness={0.5}
              />
            );
          })}
          {connecting &&
            (connecting.side === "output" ? (
              <Arrows.Arrow
                source={`${connecting.operation}:${connecting.field}`}
                target={`mouse-pointer`}
                // startAnchor="right"
                // endAnchor="left"
                // strokeWidth={2}
                // color="var(--color-stroke-normal)"
                // curveness={0.5}
              />
            ) : (
              <Arrows.Arrow
                source={`mouse-pointer`}
                target={`${connecting.operation}:${connecting.field}`}
                // startAnchor="right"
                // endAnchor="left"
                // strokeWidth={2}
                // color="var(--color-stroke-normal)"
                // curveness={0.5}
              />
            ))}
        </Arrows>
      </div>
    </OperationGridContext.Provider>
  );
};

export default OperationGrid;
export { OperationGridContext };
export type { OperationGridProps };
