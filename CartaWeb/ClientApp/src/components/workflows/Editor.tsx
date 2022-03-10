import { FC, useCallback, useEffect, useState } from "react";
import { useAPI, useNestedAsync, useStoredState } from "hooks";
import { Operation, OperationType } from "library/api";
import { seconds } from "library/utility";
import { useWorkflows } from "./Context";
import EditorNode from "./EditorNode";
import { Mosaic } from "components/mosaic";
import EditorPalette from "./EditorPalette";
import { useNotifications } from "components/notifications/Context";
import { LogSeverity } from "library/logging";
import { ITile } from "components/mosaic/Tile";
import { LoadingIcon } from "components/icons";

// TODO: Consider if the suboperations should be stored in the context.

// This represents the type of an operation while possibly being loaded.
type LoadableOperation = { id: string; operation: Operation };

const Editor: FC = () => {
  // We use a logger to log notifications.
  const { logger } = useNotifications();

  // We get the workflow to use for rendering its components.
  const { workflow } = useWorkflows();

  // Fetch the available types of operations.
  const { operationsAPI, workflowsAPI } = useAPI();
  const operationTypesFetch = useCallback(async () => {
    const types = await operationsAPI.getOperationTypes();
    return Object.fromEntries(types.map((type) => [type.type, type]));
  }, [operationsAPI]);
  const [operationTypes] = useNestedAsync<
    typeof operationTypesFetch,
    Record<string, OperationType>
  >(operationTypesFetch);

  // Fetch the suboperations of the workflow if possible.
  const suboperationsFetch = useCallback(async () => {
    if (workflow === undefined || workflow instanceof Error) return [];
    return workflow.operations.map((operationId) => ({
      id: operationId,
      operation: async () => await operationsAPI.getOperation(operationId),
    }));
  }, [operationsAPI, workflow]);
  const [suboperationsFetched, suboperationsRefresh] = useNestedAsync<
    typeof suboperationsFetch,
    LoadableOperation[]
  >(suboperationsFetch, false, seconds(30));
  const [suboperations, setSuboperations] =
    useState<typeof suboperationsFetched>(suboperationsFetched);

  // Whenever we load the operations, we need to update the list of operations.
  // We do this by buffering between loaded and loading states.
  useEffect(() => {
    // TODO: We need to make sure that we remove operations that are no longer in the workflow.
    if (
      !(
        suboperationsFetched === undefined ||
        suboperationsFetched instanceof Error
      )
    ) {
      setSuboperations((suboperations) => {
        // If we are initially loading, the buffers should be equivalent.
        if (suboperations === undefined || suboperations instanceof Error)
          return suboperationsFetched;

        // We avoid setting operations that were loaded previously but are being refreshed.
        for (const suboperationFetched of suboperationsFetched) {
          const index = suboperations.findIndex(
            (op) => op.id === suboperationFetched.id
          );
          if (index === -1)
            suboperations = [...suboperations, suboperationFetched];
          else if (suboperationFetched.operation) {
            suboperations = [
              ...suboperations.slice(0, index),
              suboperationFetched,
              ...suboperations.slice(index + 1),
            ];
          }
        }
        return suboperations;
      });
    }
  }, [suboperationsFetched]);

  // We store the positions and sizes of each of the operation tiles in local storage.
  let workflowLayoutKey: string | undefined = undefined;
  if (workflow && !(workflow instanceof Error))
    workflowLayoutKey = `workflow-layout-${workflow.id}`;
  const [operationLayouts, setOperationLayouts] = useStoredState<
    Record<string, ITile>
  >({}, workflowLayoutKey);
  const defaultDimensions: [number, number] = [8, 6];

  // Store a menu position. When this position is non-null, the menu is open.
  const [palettePosition, setPalettePosition] = useState<{
    x: number;
    y: number;
  } | null>(null);

  // We use these handlers for events on the mosaic.
  const handleMenu = useCallback(
    (event: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
      // If we have a right click, capture the the position of the click and open the palette.
      if (event.button === 2) {
        setPalettePosition({
          x: event.nativeEvent.offsetX,
          y: event.nativeEvent.offsetY,
        });
      }

      event.preventDefault();
    },
    []
  );
  const handlePickOperation = useCallback(
    async (template: { type: string; subtype: string | null } | null) => {
      if (workflow && !(workflow instanceof Error) && template) {
        // We create a new operation and add it to the workflow.
        try {
          const operation = await operationsAPI.createOperation(
            template.type,
            template.subtype
          );
          await workflowsAPI.addWorkflowOperation(workflow.id, operation.id);
          await suboperationsRefresh();

          // We use the palette position to position the new operation.
        } catch (error: any) {
          logger.log({
            source: "Workflow Editor",
            severity: LogSeverity.Error,
            title: "Operation Creation Error",
            message:
              "An error occurred while trying to create the new operation.",
            data: error,
          });
        }
      }
      setPalettePosition(null);
    },
    [logger, suboperationsRefresh, workflow, operationsAPI, workflowsAPI]
  );
  const handleLayoutOperation = useCallback(
    (id: string, layout: ITile) => {
      setOperationLayouts((operationLayouts) => ({
        ...operationLayouts,
        [id]: layout,
      }));
    },
    [setOperationLayouts]
  );

  return (
    <Mosaic onContextMenu={handleMenu}>
      {/* Render the editor palette if it has a position set.*/}
      {palettePosition && (
        <EditorPalette
          position={palettePosition}
          onPick={handlePickOperation}
        />
      )}

      {/* Render the suboperations of the workflow. */}
      {!(suboperations instanceof Error) &&
        suboperations &&
        suboperations.map((operation) => {
          if (operation === undefined || operation instanceof Error)
            return null;
          if (operationTypes === undefined || operationTypes instanceof Error)
            return null;

          // We fetch the layout information about the operation.
          let layout = operationLayouts[operation.id] || {
            position: [0, 0],
            dimensions: defaultDimensions,
          };

          const operationInstance = operation.operation;
          return (
            <Mosaic.Tile
              position={layout.position}
              dimensions={layout.dimensions}
              onLayoutChanged={(position, dimensions) => {
                handleLayoutOperation(operation.id, { position, dimensions });
              }}
            >
              {!operationInstance && (
                <div
                  style={{
                    width: "100%",
                    height: "100%",
                    display: "flex",
                    flexDirection: "column",
                    alignItems: "center",
                    justifyContent: "center",
                    backgroundColor: "var(--color-fill-element)",
                    borderRadius: "var(--border-radius)",
                  }}
                >
                  <LoadingIcon />
                </div>
              )}
              {operationInstance && !(operationInstance instanceof Error) && (
                <EditorNode
                  key={operation.id}
                  operation={operationInstance}
                  type={operationTypes[operationInstance.type]}
                  onOffset={(offset) =>
                    handleLayoutOperation(operation.id, {
                      ...layout,
                      position: [
                        layout.position[0] + offset[0],
                        layout.position[1] + offset[1],
                      ],
                    })
                  }
                />
              )}
            </Mosaic.Tile>
          );
        })}
    </Mosaic>
  );
};

export default Editor;
