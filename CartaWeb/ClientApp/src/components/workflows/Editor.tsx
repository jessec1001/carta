import React, {
  FC,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { useAPI, useNestedAsync, useStoredState } from "hooks";
import { Operation, OperationType } from "library/api";
import { LogSeverity } from "library/logging";
import { schemaDefault } from "library/schema";
import { seconds } from "library/utility";
import { useNotifications } from "components/notifications";
import { Mosaic } from "components/mosaic";
import { ITile } from "components/mosaic/Tile";
import { Arrows } from "components/arrows";
import { IconButton } from "components/buttons";
import ExecuteIcon from "components/icons/ExecuteIcon";
import { Tooltip } from "components/tooltip";
import { useWorkflows } from "./Context";
import EditorPalette from "./EditorPalette";
import EditorOperationNode from "./EditorOperationNode";
import EditorOutputNode from "./EditorOutputNode";
import styles from "./Editor.module.css";

// TODO: Consider if the suboperations should be stored in the context.

// This represents the type of an operation while possibly being loaded.
type LoadableOperation = {
  id: string;
  updating: boolean;
  operation: Operation;
};

/** A component that renders an editor of a workflow. */
const Editor: FC = () => {
  // We use a logger to log notifications.
  const { logger } = useNotifications();

  // We get the workflow to use for rendering its components.
  const { workflow, operation, job } = useWorkflows();

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
    if (workflow.value === undefined || workflow.value instanceof Error)
      return [];
    return workflow.value.operations.map((operationId) => ({
      id: operationId,
      updating: false,
      operation: async () => await operationsAPI.getOperation(operationId),
    }));
  }, [operationsAPI, workflow]);
  const [suboperationsFetched, suboperationsRefresh] = useNestedAsync<
    typeof suboperationsFetch,
    LoadableOperation[]
  >(suboperationsFetch, false, seconds(30));
  const [suboperations, setSuboperations] =
    useState<typeof suboperationsFetched>(suboperationsFetched);

  // We keep track of suboperations and connections that have been selected.
  const [suboperationsSelected, setSuboperationsSelected] = useState<
    Set<string>
  >(new Set());
  const [connectionsSelected, setConnectionsSelected] = useState<Set<string>>(
    new Set()
  );

  // Whenever we load the operations, we need to update the list of operations.
  // We do this by buffering between loaded and loading states.
  useEffect(() => {
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

        // We need to make sure that we remove operations that are no longer in the workflow.
        for (let i = 0; i < suboperations.length; i++) {
          let found: boolean = false;
          for (let j = 0; j < suboperationsFetched.length; j++) {
            if (suboperationsFetched[j].id === suboperations[i].id) {
              found = true;
              break;
            }
          }
          if (!found) suboperations.splice(i);
        }

        return suboperations;
      });
    }
  }, [suboperationsFetched]);

  // We store the positions and sizes of each of the operation tiles in local storage.
  let workflowLayoutKey: string | undefined = undefined;
  if (workflow.value && !(workflow.value instanceof Error))
    workflowLayoutKey = `workflow-layout-${workflow.value.id}`;
  const [operationLayouts, setOperationLayouts] = useStoredState<
    Record<string, ITile>
  >({}, workflowLayoutKey);
  const defaultDimensions: [number, number] = [8, 6];

  // We store information about connections between fields.
  const [activeConnection, setActiveConnection] = useState<{
    operation: string;
    field: string;
  } | null>(null);

  // Store a menu position. When this position is non-null, the menu is open.
  const element = useRef<HTMLDivElement>(null);
  const [palettePosition, setPalettePosition] = useState<{
    x: number;
    y: number;
  } | null>(null);

  // We use these handlers for events in the editor.
  const handleMenu = useCallback(
    (event: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
      // If we have a right click, capture the the position of the click and open the palette.
      if (!element.current) return;
      if (event.button === 2) {
        const rect = element.current.getBoundingClientRect();
        setPalettePosition({
          x: event.clientX - rect.left,
          y: event.clientY - rect.top,
        });
      }

      event.preventDefault();
    },
    []
  );
  const handlePickOperation = useCallback(
    async (template: { type: string; subtype: string | null } | null) => {
      if (workflow.value && !(workflow.value instanceof Error) && template) {
        // We create a new operation and add it to the workflow.
        try {
          // On initially creating a node, we should update it with its default values.
          const operation = await operationsAPI.createOperation(
            template.type,
            template.subtype
          );
          const operationSchema = await operationsAPI.getOperationSchema(
            operation.id
          );
          const operationDefaults: Record<string, any> = {};
          for (const [field, fieldSchema] of Object.entries(
            operationSchema.inputs
          )) {
            operationDefaults[field] = schemaDefault(fieldSchema);
          }
          await operationsAPI.updateOperation({
            id: operation.id,
            default: operationDefaults,
          });

          // Add the new operation to the workflow.
          await workflowsAPI.addWorkflowOperation(
            workflow.value.id,
            operation.id
          );
          await suboperationsRefresh();
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
  const handleDeleteOperations = useCallback(async () => {
    const workflowValue = workflow.value;
    if (!workflowValue || workflowValue instanceof Error) return;

    // For each of the selected connections and operations, remove each from the workflow and delete.
    // We need to remove the connections before the operations because the API automatically removes relevant connections.
    const opIds = Array.from(suboperationsSelected.values());
    const connIds = Array.from(connectionsSelected.values());
    const resultsConns = await Promise.allSettled(
      connIds.map(async (connId: string) => {
        await workflowsAPI.removeWorkflowConnection(workflowValue.id, connId);
      })
    );
    const resultsOps = await Promise.allSettled(
      opIds.map(async (opId: string) => {
        await workflowsAPI.removeWorkflowOperation(workflowValue.id, opId);
        await operationsAPI.deleteOperation(opId);
      })
    );

    // Clear the selection.
    setSuboperationsSelected(new Set());
    setConnectionsSelected(new Set());

    // Check each of the result statuses to determine whether the deletions were handled properly.
    for (let k = 0; k < connIds.length; k++) {
      if (resultsConns[k].status === "rejected") {
        logger.log({
          source: "Workflow Editor",
          severity: LogSeverity.Error,
          title: "Connection Deletion Error",
          message: "An error occurred while trying to delete a connection.",
        });
      }
    }
    for (let k = 0; k < opIds.length; k++) {
      if (resultsOps[k].status === "rejected") {
        logger.log({
          source: "Workflow Editor",
          severity: LogSeverity.Error,
          title: "Operation Deletion Error",
          message: `An error occurred while trying to delete an operation.`,
        });
      }
    }

    await workflow.refresh();
    await suboperationsRefresh();
  }, [
    workflow,
    suboperationsSelected,
    connectionsSelected,
    suboperationsRefresh,
    workflowsAPI,
    operationsAPI,
    logger,
  ]);
  const handleLayoutOperation = useCallback(
    (id: string, layout: ITile) => {
      setOperationLayouts((operationLayouts) => ({
        ...operationLayouts,
        [id]: layout,
      }));
    },
    [setOperationLayouts]
  );
  const handlePickField = useCallback(
    async (operation: string, field: string, side: "input" | "output") => {
      // Check that the workflow is loaded.
      if (!workflow.value || workflow.value instanceof Error) return;

      // Check if we are trying to connect to the wrong side of field.
      if (!activeConnection && side === "input") return;
      if (activeConnection && side === "output") return;

      // Either start or finish the connection.
      if (side === "output") {
        setActiveConnection({ operation, field });
      }
      if (side === "input") {
        if (activeConnection) {
          // We are trying to connect to a field.
          try {
            setActiveConnection(null);
            await workflowsAPI.addWorkflowConnection(workflow.value.id, {
              source: {
                operation: activeConnection.operation,
                field: activeConnection.field,
              },
              target: {
                operation,
                field,
              },
            });
            await workflow.refresh();
          } catch (error: any) {
            logger.log({
              source: "Workflow Editor",
              severity: LogSeverity.Error,
              title: "Connection Creation Error",
              message:
                "An error occurred while trying to create the connection.",
              data: error,
            });
          }
          setActiveConnection(null);
        } else {
          // We are trying to connect to an input field.
          setActiveConnection({ operation, field });
        }
      }
    },
    [activeConnection, logger, workflow, workflowsAPI]
  );
  const handleUpdateFields = useCallback(
    async (operation: string, fields: Record<string, any>) => {
      // Check that the workflow is loaded.
      if (!workflow.value || workflow.value instanceof Error) return;

      // Check that there is a corresponding loaded suboperation.
      if (!suboperations || suboperations instanceof Error) return;
      const suboperationIndex = suboperations.findIndex(
        (op) =>
          op.operation &&
          !(op.operation instanceof Error) &&
          op.operation.id === operation
      );
      if (suboperationIndex === -1) return;
      const suboperation = suboperations[suboperationIndex]
        .operation as Operation;

      // Update the field.
      try {
        // We should indicate that the field is being updated.
        const newSuboperations = [...suboperations];
        newSuboperations[suboperationIndex] = {
          ...suboperations[suboperationIndex],
          updating: true,
          operation: {
            ...suboperation,
            default: {
              ...suboperation.default,
              ...fields,
            },
          },
        };
        setSuboperations(newSuboperations);

        // Update the field.
        await operationsAPI.updateOperation({
          id: operation,
          default: {
            ...suboperation.default,
            ...fields,
          },
        });
        await suboperationsRefresh();
        await workflow.refresh();
      } catch (error: any) {
        logger.log({
          source: "Workflow Editor",
          severity: LogSeverity.Error,
          title: "Field Update Error",
          message: "An error occurred while trying to update the field.",
          data: error,
        });
      }
    },
    [workflow, suboperations, suboperationsRefresh, logger, operationsAPI]
  );
  const handleExecuteOperation = useCallback(async () => {
    if (!operation.value || operation.value instanceof Error) return;

    try {
      const jobInstance = await operationsAPI.executeOperation(
        operation.value.id,
        {}
      );
      job.set(jobInstance.id);
    } catch (error: any) {
      logger.log({
        source: "Workflow Editor",
        severity: LogSeverity.Error,
        title: "Operation Execution Error",
        message: "An error occurred while trying to execute the operation.",
        data: error,
      });
    }
  }, [logger, job, operation, operationsAPI]);
  const handleClickGrid = useCallback((event: React.MouseEvent) => {
    // If the element that was clicked was the mosaic grid itself, deselect suboperation nodes.
    if (event.currentTarget === event.target) {
      setSuboperationsSelected(new Set());
      setConnectionsSelected(new Set());
    }
  }, []);
  const handleClickOperation = useCallback((id: string) => {
    setSuboperationsSelected((selected) => {
      const selectedNew = new Set(selected);
      selectedNew.add(id);
      return selectedNew;
    });
  }, []);
  const handleUnclickOperation = useCallback((id: string) => {
    setSuboperationsSelected(new Set());
    setConnectionsSelected(new Set());
  }, []);
  const handleClickConnection = useCallback((id: string) => {
    console.log(id);
    setConnectionsSelected((selected) => {
      const selectedNew = new Set(selected);
      selectedNew.add(id);
      return selectedNew;
    });
  }, []);

  // We set up a keyboard shortcut handler.
  useEffect(() => {
    const handleKeydown = (event: KeyboardEvent) => {
      if (event.code === "Backspace" || event.code === "Delete") {
        handleDeleteOperations();
      }
      if (event.code === "Tab") {
        setSuboperationsSelected(new Set());
        setConnectionsSelected(new Set());
      }
    };

    // Setup and teardown.
    document.addEventListener("keydown", handleKeydown);
    return () => document.removeEventListener("keydown", handleKeydown);
  }, [handleDeleteOperations]);

  // Create an information element for navigating the workflow editor.
  const info = useMemo(() => {
    return (
      <div className={styles.buttonsTooltip}>
        <ul>
          This is the Workflow Editor view. Here you can construct and execute a
          workflow.
          <li>
            Right click to open the operations pallete to select an operation to
            create.
          </li>
          <li>
            Click and drag the title of an operation to move it around the
            workflow.
          </li>
          <li>Click and drag on the background to pan around the workflow.</li>
          <li>
            Attach connections by clicking on an output point of some operation
            and subsequently clicking on an input point of another operation.
          </li>
          <li>Press the button below to execute the workflow.</li>
        </ul>
      </div>
    );
  }, []);

  return (
    <Mosaic onContextMenu={handleMenu} onClick={handleClickGrid} ref={element}>
      {/* We wrap everything in an arrows component so as to render the connections. */}
      <Arrows element={element.current}>
        {/* Render the info and execute button in a fixed-position menu. */}
        <div className={styles.buttons}>
          <Tooltip component={info} options={{ placement: "left" }} hover>
            <IconButton className={styles.button}>i</IconButton>
          </Tooltip>
          <IconButton
            className={styles.button}
            onClick={handleExecuteOperation}
          >
            <ExecuteIcon />
          </IconButton>
        </div>

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
            // Assume that the operation type is either unloaded or loaded, never errored.
            let layout = operationLayouts[operation.id] || {
              position: [0, 0],
              dimensions: defaultDimensions,
            };

            const operationInstance = operation.operation;
            const operationType =
              !operationInstance || operationInstance instanceof Error
                ? undefined
                : operationTypes[operationInstance.type];
            return (
              <Mosaic.Tile
                key={operation.id}
                position={layout.position}
                dimensions={layout.dimensions}
                onLayoutChanged={(position, dimensions) => {
                  handleLayoutOperation(operation.id, { position, dimensions });
                }}
                resizeable
              >
                {
                  <EditorOperationNode
                    key={operation.id}
                    workflow={workflow.value}
                    operation={operationInstance}
                    type={operationType}
                    layout={layout}
                    updating={operation.updating}
                    selected={suboperationsSelected.has(operation.id)}
                    onClick={() => handleClickOperation(operation.id)}
                    onUnclick={() => handleUnclickOperation(operation.id)}
                    onOffset={(offset) =>
                      handleLayoutOperation(operation.id, {
                        ...layout,
                        position: [
                          layout.position[0] + offset[0],
                          layout.position[1] + offset[1],
                        ],
                      })
                    }
                    onPickField={(field, side) => {
                      handlePickField(operation.id, field, side);
                    }}
                    onUpdateFields={(fields) => {
                      handleUpdateFields(operation.id, fields);
                    }}
                  />
                }
              </Mosaic.Tile>
            );
          })}

        {/* Render the outputs of the workflow. */}
        {!(operation.value instanceof Error) &&
          operation.value?.schema &&
          Object.entries(operation.value.schema.outputs).map(
            ([name, schema]) => {
              // We fetch the layout information about the operation.
              // Assume that the operation type is either unloaded or loaded, never errored.
              const operationIdentifier = (operation.value as Operation).id;
              const outputIdentifier = `output-${name}`;
              let layout = operationLayouts[outputIdentifier] || {
                position: [0, 0],
                dimensions: defaultDimensions,
              };

              return (
                <Mosaic.Tile
                  key={outputIdentifier}
                  position={layout.position}
                  dimensions={layout.dimensions}
                  onLayoutChanged={(position, dimensions) => {
                    handleLayoutOperation(outputIdentifier, {
                      position,
                      dimensions,
                    });
                  }}
                  resizeable
                >
                  <EditorOutputNode
                    operation={operationIdentifier}
                    job={job.value}
                    field={name}
                    schema={schema}
                    onOffset={(offset) =>
                      handleLayoutOperation(outputIdentifier, {
                        ...layout,
                        position: [
                          layout.position[0] + offset[0],
                          layout.position[1] + offset[1],
                        ],
                      })
                    }
                  />
                </Mosaic.Tile>
              );
            }
          )}

        {/* Render the connections of the workflow. */}
        {!(workflow.value instanceof Error) &&
          workflow.value &&
          workflow.value.connections.map((connection) => {
            const selected = connectionsSelected.has(connection.id);
            return (
              <Arrows.Arrow
                onClick={() => handleClickConnection(connection.id)}
                pathStyle={{
                  stroke: selected ? "var(--color-primary)" : "currentcolor",
                }}
                key={connection.id}
                source={`${connection.source.operation}-output-${connection.source.field}`}
                target={`${connection.target.operation}-input-${connection.target.field}`}
              />
            );
          })}
      </Arrows>
    </Mosaic>
  );
};

export default Editor;
