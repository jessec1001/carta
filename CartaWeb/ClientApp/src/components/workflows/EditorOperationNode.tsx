import React, {
  ComponentProps,
  FC,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import classNames from "classnames";
import { Operation, OperationType, Workflow } from "library/api";
import { JsonSchema } from "library/schema";
import { Theme, ThemeContext } from "components/theme";
import { SchemaBaseInput } from "components/form/schema";
import { Mosaic } from "components/mosaic";
import { Tooltip } from "components/tooltip";
import { ITile } from "components/mosaic/Tile";
import ReactMarkdown from "react-markdown";
import { LoadingIcon } from "components/icons";
import { Text } from "components/text";
import { Arrows } from "components/arrows";
import styles from "./EditorOperationNode.module.css";
import { useDelayCallback } from "hooks";
import EditorNode from "./EditorNode";

// TODO: Add popper message to hovering over the tag strips of the node.
//       This popper should contain links for tags that immediately open the operation palette to that tag.
// TODO: Reintegrate logic for visualizing any data that may be associated with the node.
//       This might better be placed in another visualization component.

/**
 * Computes the hue and saturation values of the color that should represent a tag.
 * @param tag The operation tag.
 * @returns A tuple of hue and saturation values.
 */
const computeTagHueSat = (tag: string): [hue: number, sat: number] => {
  switch (tag.toLowerCase()) {
    // #region Internal
    case "workflow":
      return [25, 0.5];
    // #endregion

    // #region Data Algorithms
    case "synthetic":
      return [175, 1.0];
    case "loading":
      return [195, 1.0];
    case "saving":
      return [200, 1.0];
    case "parsing":
      return [215, 1.0];
    case "conversion":
      return [225, 1.0];
    case "visualization":
      return [235, 1.0];
    // #endregion

    // #region Data Structures
    case "text":
      return [55, 1.0];
    case "array":
      return [70, 1.0];
    case "graph":
      return [85, 1.0];
    // #endregion

    // #region Integration
    case "hyperthought":
      return [240, 0.5];
    case "carta":
      return [100, 0.5];
    // #endregion

    // #region Mathematical Fields
    case "arithmetic":
      return [0, 0.8];
    case "statistics":
      return [20, 0.8];
    case "numbertheory":
      return [300, 0.8];
    // #endregion

    default:
      return [0, 0];
  }
};
/**
 * Computes the color that should represent a tag.
 * @param tag The operation tag.
 * @param theme The current theme.
 * @returns The CSS color value.
 */
const computeTagColor = (tag: string, theme: Theme): string => {
  const [hue, sat] = computeTagHueSat(tag);
  let lit;
  switch (theme) {
    case Theme.Light:
      lit = 0.75;
      break;
    case Theme.Dark:
      lit = 0.25;
      break;
  }
  return `hsl(${hue}, ${100 * sat}%, ${100 * lit}%)`;
};

/** The props used for the {@link EditorOperationNodeStripes} component. */
interface EditorOperationNodeStripesProps {
  /** The tags of the operation to render as stripes. */
  tags: string[];
}
/** A component that renders the stripes representing tags for a node. */
const EditorOperationNodeStripes: FC<EditorOperationNodeStripesProps> = ({
  tags,
}) => {
  // We depend on the theme for coloring the stripes.
  const { theme } = useContext(ThemeContext);

  return (
    <div className={styles.stripes}>
      {tags.map((tag) => (
        <div
          key={tag}
          className={styles.stripe}
          style={{
            background: computeTagColor(tag, theme),
          }}
        />
      ))}
    </div>
  );
};

/** The props used for the {@link EditorOperationNodeField} component. */
interface EditorOperationNodeFieldProps {
  /** The unique identifier for the operation field in the workflow. */
  id: string;
  /** The key for the field of the operation. */
  field: string;
  /** The side of the node that this connector should be on. */
  side: "input" | "output";
  /** The schema to render for the field connector. */
  schema: JsonSchema;
  /** The value assigned to the field. */
  value: any;

  /** Whether the field should be editable. */
  editable: boolean;

  /** An event listener that is called when a field connection point is picked. */
  onPickField?: (field: string, side: "input" | "output") => void;
  /** An event listener that is called when a field is updated. */
  onUpdateField?: (field: string, side: "input" | "output", value: any) => void;
}
/** A component that renders a particular operation field for a node. */
const EditorOperationNodeField: FC<EditorOperationNodeFieldProps> = ({
  id,
  field,
  side,
  schema,
  value,
  editable,
  onPickField = () => {},
  onUpdateField = () => {},
}) => {
  // We create a documentation tooltip for the field.
  const documentation = (
    <ReactMarkdown>{schema.description ?? ""}</ReactMarkdown>
  );

  return (
    <div className={classNames(styles.field, styles[side])}>
      <Arrows.Node
        id={id}
        pollingInterval={25}
        onClick={() => onPickField(field, side)}
        className={styles.fieldPoint}
      />
      <Tooltip
        className={styles.fieldLabel}
        component={documentation}
        options={{
          placement: side === "input" ? "left" : "right",
        }}
        hover
      >
        {schema.title}
      </Tooltip>

      {/* TODO: Display error information and verify schema. */}
      {/* TODO: Implement a debugging view that can display partial results. */}
      {/* We only render the schema for the input parameters. */}
      {editable && side === "input" && (
        <div className={styles.fieldInput}>
          <SchemaBaseInput
            schema={schema}
            value={value}
            onChange={(value) => onUpdateField(field, side, value)}
          />
        </div>
      )}
    </div>
  );
};

/** The props for the {@link EditorOperationNode} component. */
interface EditorOperationNodeProps extends ComponentProps<"div"> {
  /** The workflow that the operation lives inside. */
  workflow?: Workflow | Error;
  /** The operation instance to render in the node. */
  operation?: Operation | Error;
  /** The operation type to render. */
  type?: OperationType;

  /** Whether the node is currently selected. */
  selected?: boolean;
  /** Whether the node is currently updating. */
  updating?: boolean;

  /** The layout of the node. Relevant for updating connections and tooltips. */
  layout?: ITile;

  /** An event listener that is called when the node is offset on the grid. */
  onOffset?: (offset: [number, number]) => void;
  /** An event listener that is called when a field connection point is picked. */
  onPickField?: (field: string, side: "input" | "output") => void;
  /** An event listener that is called when a field is updated. */
  onUpdateFields?: (fields: Record<string, any>) => void;
}
/** A component that renders an operation node in the workflow editor. */
const EditorOperationNode: FC<EditorOperationNodeProps> = ({
  workflow,
  operation,
  type,
  selected,
  updating,
  layout,
  onOffset = () => {},
  onPickField = () => {},
  onUpdateFields = () => {},
  className,
  ...props
}) => {
  // We buffer the value so that input may occur without delay.
  const operationDefaults = useMemo(
    () =>
      !operation || operation instanceof Error ? {} : operation.default ?? {},
    [operation]
  );
  const [actualValues, setValues] =
    useState<Record<string, any>>(operationDefaults);
  useEffect(() => {
    setValues(operationDefaults);
  }, [operationDefaults]);

  // We use a delay when updating the field to prevent too many API calls with race conditions.
  const handleUpdateFields = useDelayCallback(onUpdateFields, 1000, true);
  const handleUpdateField = useCallback(
    (field: string, side: "input" | "output", value: any) => {
      setValues((values) => ({
        ...values,
        [field]: value,
      }));
      handleUpdateFields({
        ...actualValues,
        [field]: value,
      });
    },
    [actualValues, handleUpdateFields]
  );

  // If the operation is not loaded yet, render a loading symbol.
  if (!operation || !type) {
    return <EditorNode loading />;
  }

  // If the operation has an error, render an error message.
  if (operation instanceof Error) {
    return (
      <EditorNode>
        <Text color="error">{operation.message}</Text>
      </EditorNode>
    );
  }

  // We create a utility function to check if a field should be rendered completely.
  const isFieldConnected = (
    field: string,
    side: "input" | "output"
  ): boolean => {
    if (!workflow || workflow instanceof Error) return false;

    const { connections } = workflow;
    for (const connection of connections) {
      if (
        side === "input" &&
        connection.target.operation === operation.id &&
        connection.target.field === field
      ) {
        return true;
      }
      if (
        side === "output" &&
        connection.source.operation === operation.id &&
        connection.source.field === field
      ) {
        return true;
      }
    }
    return false;
  };

  // We create a documentation tooltip for the operation.
  const documentation = (
    <ReactMarkdown linkTarget="_blank">{type.description ?? ""}</ReactMarkdown>
  );

  return (
    <EditorNode selected={selected}>
      {/* Render a header with the operation name and */}
      <Mosaic.Tile.Handle onOffset={onOffset}>
        <Tooltip
          className={styles.header}
          component={documentation}
          options={{ placement: "right" }}
          hover
        >
          <span>{type.display}</span>
          <div className={styles.headerTools}>
            {updating && <LoadingIcon />}
            <EditorOperationNodeStripes tags={type.tags} />
          </div>
        </Tooltip>
      </Mosaic.Tile.Handle>
      <div className={styles.body}>
        {/* Render the input schema. */}
        {operation.schema &&
          Object.entries(operation.schema.inputs).map(([key, val]) => (
            <EditorOperationNodeField
              key={`input-${key}`}
              id={`${operation.id}-input-${key}`}
              field={key}
              side="input"
              schema={val}
              value={actualValues[key]}
              editable={!isFieldConnected(key, "input")}
              onPickField={onPickField}
              onUpdateField={handleUpdateField}
            />
          ))}
        {/* Render the output schema. */}
        {operation.schema &&
          Object.entries(operation.schema.outputs).map(([key, val]) => (
            <EditorOperationNodeField
              key={`output-${key}`}
              id={`${operation.id}-output-${key}`}
              field={key}
              side="output"
              schema={val}
              value={undefined}
              editable={!isFieldConnected(key, "output")}
              onPickField={onPickField}
              onUpdateField={handleUpdateField}
            />
          ))}
      </div>
    </EditorNode>
  );
};

export default EditorOperationNode;
export type { EditorOperationNodeProps };
