import { SchemaBaseInput } from "components/form/schema";
import { Operation, OperationType } from "library/api";
import { useCallback, useEffect, useState } from "react";
import { FunctionComponent } from "react";
import LockIcon from "components/icons/LockIcon";
import "./OperationNode.css";
import OperationNodeConnector from "./OperationNodeConnector";
import { useRef } from "react";
import { ScatterPlot, HistogramPlot, GraphPlot } from "visualize-carta";
import { useContext } from "react";
import { Theme, ThemeContext } from "components/theme";
import { useWorkflow } from "./WorkflowContext";
import classNames from "classnames";
import { OperationGridContext } from "./OperationGrid";
import { useAPI, useDelayCallback } from "hooks";
import { VisualizeIcon } from "components/icons";
import { useViews } from "components/views";
import VisualizerView from "../VisualizerView";
import { Link } from "components/link";

interface OperationNodeProps {
  operation: Operation;
  type: OperationType;
  data?: any;
}

const Visualizer: FunctionComponent<{
  workflowOperation: Operation;
  operation: Operation;
  data: any;
  type: string;
}> = ({ workflowOperation: workflow, operation, data, type }) => {
  const ref = useRef<HTMLDivElement>(null);
  const { operationsAPI } = useAPI();
  const { workflowOperation, setInputField, setFile, setFileField } =
    useWorkflow();
  const { jobId } = useWorkflow();

  useEffect(() => {
    if (!ref.current) return;

    if (data && data.type) {
      while (ref.current.firstChild) {
        ref.current.removeChild(ref.current.firstChild);
      }
    }

    if (data) {
      switch (data.type) {
        case "scatter":
          ScatterPlot(ref.current, {
            ...data,
            size: { width: 400, height: 400 },
          });
          break;
        case "histogram":
          HistogramPlot(ref.current, {
            ...data,
            size: { width: 400, height: 400 },
          });
          break;
        case "graph":
          GraphPlot(ref.current, {
            ...data,
            size: { width: 400, height: 400 },
          });
          break;
      }
    }
    // TODO: Revert once re-rendering issue has been solved
  }, [type, data]);

  // const inputType =
  //   type === "input" &&
  //   operation.default &&
  //   operation.default["Name"] &&
  //   workflow.schema &&
  //   workflow.schema.inputs[operation.default["Name"]] &&
  //   workflow.schema.inputs[operation.default["Name"]].type;
  // const inputUpload =
  //   inputType && (inputType === "file" || inputType.includes("file"));
  const outputType =
    type === "output" &&
    operation.default &&
    operation.default["Name"] &&
    workflow.schema &&
    workflow.schema.outputs[operation.default["Name"]] &&
    workflow.schema.outputs[operation.default["Name"]].type;
  const outputDownload =
    outputType && (outputType === "file" || outputType.includes("file"));

  return (
    <div
      className="OperationNode"
      ref={ref}
      style={
        type === "output" || type === "input"
          ? { padding: "0.5rem" }
          : { width: "400px", height: "400px", overflow: "hidden" }
      }
    >
      {type === "output" &&
        (outputDownload ? (
          <Link
            to="#"
            onClick={(e) => {
              operationsAPI.downloadJobFile(
                workflow.id,
                jobId!,
                operation.default!["Name"]
              );
              e.preventDefault();
            }}
          >
            Download
          </Link>
        ) : (
          <pre>{JSON.stringify(data, null, 2)}</pre>
        ))}
      {/* TODO: Convert into auto form. */}
      {type === "input" &&
        workflowOperation?.schema &&
        workflowOperation.schema.inputs[data] && (
          <SchemaBaseInput
            schema={workflowOperation.schema.inputs[data]}
            onChange={(value) => {
              const fileLike =
                workflowOperation.schema?.inputs[data].type === "file" ||
                (Array.isArray(workflowOperation.schema?.inputs?.[data].type) &&
                  workflowOperation.schema?.inputs?.[data].type?.includes(
                    "file"
                  ));

              if (fileLike) {
                value = value as File | null;
                if (value === null) {
                  setFile(null);
                  setFileField(null);
                  setInputField(data, false);
                } else {
                  setFile(value);
                  setFileField(data);
                  setInputField(data, true);
                }
              } else {
                setInputField(data, value);
              }
            }}
          />
        )}
    </div>
  );
};

const OperationNode: FunctionComponent<OperationNodeProps> = ({
  type,
  operation,
  data,
}) => {
  const [updater, setUpdater] = useState<null | ((data: any) => void)>(() => {
    return null;
  });
  const { rootId, actions: viewActions } = useViews();
  const [defaults, setDefaults] = useState<Record<string, any>>(
    operation.default ?? {}
  );
  const { workflow, selected, select, updateOperation, workflowOperation } =
    useWorkflow();

  const { setVerticalPosition } = useContext(OperationGridContext)!;
  const [startPosition, setStartPosition] = useState<number | null>(null);
  const elementRef = useRef<HTMLDivElement>(null);

  // If the operation defaults have changed, update them within the state.
  useEffect(() => {
    setDefaults(operation.default ?? {});
  }, [operation.default]);

  useEffect(() => {
    const handleMouseMove = (event: MouseEvent) => {
      setStartPosition((position) => {
        if (position === null) return null;
        setVerticalPosition(operation.id, position + event.movementY);
        return position + event.movementY;
      });
    };
    const handleMouseUp = () => {
      setStartPosition(null);
    };

    window.addEventListener("mousemove", handleMouseMove);
    window.addEventListener("mouseup", handleMouseUp);
    return () => {
      window.removeEventListener("mousemove", handleMouseMove);
      window.removeEventListener("mouseup", handleMouseUp);
    };
  }, [operation.id, setVerticalPosition, startPosition]);

  useEffect(() => {
    if (updater) {
      updater(data);
    }
  }, [data, updater]);

  const { theme } = useContext(ThemeContext);

  // When any of the operation inputs is changed, we wait for inputs to stop before dispatching the update.
  const handleUpdate = useDelayCallback(updateOperation, 2500);
  const handleModify = (key: string, value: any) => {
    setDefaults((defaults) => {
      const newDefaults = {
        ...defaults,
        [key]: value,
      };

      handleUpdate(operation.id, newDefaults);
      return newDefaults;
    });
  };

  const handleVisualizer = useCallback((fn: null | ((data: any) => void)) => {
    setUpdater(() => {
      return fn;
    });
  }, []);

  return (
    <>
      <div
        ref={elementRef}
        className={classNames("OperationNode", {
          selected: selected.some(
            (selectedOperation) => selectedOperation.id === operation.id
          ),
        })}
      >
        <div
          title={type.description ?? ""}
          className="title"
          style={{
            background: computeLinearGradient(
              type.tags.map((tag) => findTagColor(tag, theme))
            ),
          }}
          onClick={() => select([operation])}
          onMouseDown={(event) => {
            setStartPosition(
              (elementRef.current!.parentElement as any).offsetTop
            );
          }}
        >
          <span>{type.display}</span>
          {/* TODO: It would be nice if icons were clickable. */}
          {/* TODO: Allow for multiple visualizers to be opened. */}
          {/* For visualizer types, a visualize button should be displayed. */}
          {type.type === "workflowOutput" && data && (
            <span
              style={{
                display: "flex",
                alignItems: "center",
              }}
              // TODO: Figure out some better way to forward data.
              onClick={() => {
                viewActions.addElementToContainer(
                  rootId,
                  <VisualizerView
                    type={data.type}
                    name={operation.default!["Name"]}
                    onUpdate={handleVisualizer}
                    onClose={() => {
                      setUpdater(null);
                    }}
                  />,
                  true
                );
              }}
            >
              <VisualizeIcon />
            </span>
          )}
          {/* TODO: Reimplement (workflow locked symbol) */}
          {false && <LockIcon />}
        </div>
        <div className="body">
          {operation.schema?.inputs &&
            Object.entries(operation.schema.inputs ?? {}).map(
              ([key, value]) => {
                const displayField = !workflow?.connections.some(
                  (connection) =>
                    connection.target.operation === operation.id &&
                    connection.target.field === key
                );

                return (
                  <div
                    key={key}
                    style={{
                      paddingRight: "0.5rem",
                      display: "flex",
                      padding: "0.25rem 0rem",
                    }}
                  >
                    <OperationNodeConnector
                      operation={operation}
                      connector={{ name: key, type: value.type as string }}
                      attachment="input"
                    />
                    {displayField && (
                      <div style={{ flexGrow: 1 }}>
                        <SchemaBaseInput
                          schema={value}
                          value={defaults![key]}
                          onChange={(value: any) => handleModify(key, value)}
                        />
                      </div>
                    )}
                  </div>
                );
              }
            )}
          {operation.schema?.outputs &&
            Object.entries(operation.schema.outputs ?? {}).map(
              ([key, value]) => {
                return (
                  <OperationNodeConnector
                    operation={operation}
                    key={key}
                    connector={{ name: key, type: value.type as string }}
                    attachment="output"
                  />
                );
              }
            )}
        </div>
      </div>
      {/* TODO: Transition to using visualization names provided by the server. */}
      {!updater && operation.type === "workflowOutput" && (
        <Visualizer
          workflowOperation={workflowOperation}
          operation={operation}
          data={data}
          type="output"
        />
      )}
      {!updater && operation.type === "workflowInput" && (
        <Visualizer
          workflowOperation={workflowOperation}
          operation={operation}
          data={operation.default?.Name}
          type="input"
        />
      )}
    </>
  );
};

const findTagColor = (tag: string, theme: Theme): string => {
  const [hue, sat] = findTagHueSat(tag);
  const lit = theme === Theme.Light ? 0.75 : 0.25;
  return `hsl(${hue}, ${100 * sat}%, ${100 * lit}%)`;
};
const findTagHueSat = (tag: string): [number, number] => {
  switch (tag.toLowerCase()) {
    // #region Data Algorithms
    case "generation":
      return [175, 1.0];
    case "loading":
      return [195, 1.0];
    case "parsing":
      return [215, 1.0];
    case "visualization":
      return [235, 1.0];
    // #endregion

    // #region Data Structures
    case "array":
      return [50, 1.0];
    case "graph":
      return [85, 1.0];
    // #endregion

    // #region Mathematical Fields
    case "arithmetic":
      return [0, 0.8];
    case "statistics":
      return [20, 0.8];
    case "numbertheory":
      return [300, 0.8];
    // #endregion

    // #region Internal
    case "workflow":
      return [25, 0.5];
    default:
      return [0, 0];
    // #endregion
  }
};

const computeLinearGradient = (
  colors: string[],
  direction: string = "-0.125turn"
): string => {
  if (colors.length > 1) {
    return `linear-gradient(
    ${direction},
    ${[colors].join(",")}
  )`;
  } else {
    return colors[0];
  }
};

export default OperationNode;
export type { OperationNodeProps };
