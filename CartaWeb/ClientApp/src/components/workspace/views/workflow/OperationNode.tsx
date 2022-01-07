import { SchemaBaseInput } from "components/form/schema";
import { Operation, OperationType } from "library/api";
import { JsonObjectSchema } from "library/schema";
import { useEffect, useState } from "react";
import { FunctionComponent } from "react";
import LockIcon from "components/icons/LockIcon";
import "./OperationNode.css";
import OperationNodeConnector from "./OperationNodeConnector";
import { useRef } from "react";
import { ScatterPlot, HistogramPlot } from "visualize-carta";
import { useContext } from "react";
import { Theme, ThemeContext } from "context";
import { useWorkflow } from "./WorkflowContext";
import classNames from "classnames";
import { OperationGridContext } from "./OperationGrid";
import { useDelayCallback } from "hooks";

interface OperationNodeProps {
  operation: Operation;
  type: OperationType;
  data?: any;
}

const Visualizer: FunctionComponent<{ data: any; type: string }> = ({
  data,
  type,
}) => {
  const ref = useRef<HTMLDivElement>(null);
  const { workflowOperation, setInputField, setFile, setFileField } =
    useWorkflow();

  useEffect(() => {
    if (!ref.current) return;

    if (type !== "output" && type !== "input") {
      while (ref.current.firstChild) {
        ref.current.removeChild(ref.current.firstChild);
      }
    }

    if (data) {
      switch (type) {
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
      }
    }
    // TODO: Revert once re-rendering issue has been solved
  }, [type, data]);

  return (
    <div
      className="OperationNode"
      ref={ref}
      style={
        type === "output" || type === "input"
          ? { padding: "0.5rem" }
          : { width: "400px", height: "400px" }
      }
    >
      {type === "output" && <pre>{JSON.stringify(data, null, 2)}</pre>}
      {/* TODO: Convert into auto form. */}
      {type === "input" &&
        workflowOperation?.input &&
        (workflowOperation.input as JsonObjectSchema).properties &&
        (workflowOperation.input as JsonObjectSchema).properties![data] && (
          <SchemaBaseInput
            schema={
              (workflowOperation.input as JsonObjectSchema).properties![data]
            }
            onChange={(value) => {
              if (
                (workflowOperation.input as JsonObjectSchema).properties![data]
                  .type === "file"
              ) {
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
  console.log(operation);
  const [defaults, setDefaults] = useState<Record<string, any>>(
    operation.default ?? {}
  );
  const { workflow, selected, select, updateOperation, input } = useWorkflow();

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
  }, [startPosition]);

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
          {/* TODO: Reimplement (workflow locked symbol) */}
          {false && <LockIcon />}
        </div>
        <div className="body">
          {operation.input &&
            Object.entries(
              (operation.input as JsonObjectSchema).properties ?? {}
            ).map(([key, value]) => {
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
            })}
          {operation.output &&
            Object.entries(
              (operation.output as JsonObjectSchema).properties ?? {}
            ).map(([key, value]) => {
              return (
                <OperationNodeConnector
                  operation={operation}
                  key={key}
                  connector={{ name: key, type: value.type as string }}
                  attachment="output"
                />
              );
            })}
        </div>
      </div>
      {/* TODO: Transition to using visualization names provided by the server. */}
      {operation.type === "visualizeScatterPlot" && (
        <Visualizer data={data} type="scatter" />
      )}
      {operation.type === "visualizeHistogramPlot" && (
        <Visualizer data={data} type="histogram" />
      )}
      {operation.type === "workflowOutput" && (
        <Visualizer data={data} type="output" />
      )}
      {operation.type === "workflowInput" && (
        <Visualizer data={operation.default?.Name} type="input" />
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
