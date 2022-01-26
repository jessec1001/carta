import { Operation, OperationType, Workflow } from "library/api";
import React, { createContext, useEffect, useState } from "react";
import { useCallback } from "react";
import { FunctionComponent } from "react";
import Xarrow, { Xwrapper } from "react-xarrows";
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
        {workflow.operations.map((operationId) => {
          const operation = operations.find(
            (operation) => operation.id === operationId
          );
          if (!operation) return null;

          const operationType = operationTypes.find(
            (type) =>
              type.type === operation.type && type.subtype === operation.subtype
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
        <Xwrapper>
          {workflow.connections.map((connection) => {
            return (
              <Xarrow
                key={connection.id}
                start={`connector_${connection.source.operation}_${connection.source.field}`}
                end={`connector_${connection.target.operation}_${connection.target.field}`}
                dashness={connection.multiplex}
                startAnchor="right"
                endAnchor="left"
                strokeWidth={2}
                color="var(--color-stroke-normal)"
                curveness={0.5}
                passProps={{
                  style: { cursor: "pointer" },
                  onClick: (event: React.MouseEvent<SVGRectElement>) => {
                    removeConnection(connection.id).then(() => {});
                    event.preventDefault();
                  },
                }}
              />
            );
          })}
          {connecting &&
            (connecting.side === "output" ? (
              <Xarrow
                start={`connector_${connecting.operation}_${connecting.field}`}
                end="mouse-pointer"
                startAnchor="right"
                endAnchor="left"
                strokeWidth={2}
                color="var(--color-stroke-normal)"
                curveness={0.5}
              />
            ) : (
              <Xarrow
                end={`connector_${connecting.operation}_${connecting.field}`}
                start="mouse-pointer"
                startAnchor="right"
                endAnchor="left"
                strokeWidth={2}
                color="var(--color-stroke-normal)"
                curveness={0.5}
              />
            ))}
        </Xwrapper>
      </div>
    </OperationGridContext.Provider>
  );
};

export default OperationGrid;
export { OperationGridContext };
export type { OperationGridProps };
