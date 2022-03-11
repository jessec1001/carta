import React, { ComponentProps, FC, useContext } from "react";
import classNames from "classnames";
import { Operation, OperationType } from "library/api";
import { JsonSchema } from "library/schema";
import { Theme, ThemeContext } from "components/theme";
import styles from "./EditorNode.module.css";
import { SchemaBaseInput } from "components/form/schema";
import { Mosaic } from "components/mosaic";
import { Tooltip } from "components/tooltip";
import { ITile } from "components/mosaic/Tile";
import ReactMarkdown from "react-markdown";
import { LoadingIcon } from "components/icons";
import { Text } from "components/text";
import { Arrows } from "components/arrows";

// TODO: Add popper message to hovering over the title of the node.
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

/** The props used for the {@link EditorNodeStripes} component. */
interface EditorNodeStripesProps {
  /** The tags of the operation to render as stripes. */
  tags: string[];
}
/** A component that renders the stripes representing tags for a node. */
const EditorNodeStripes: FC<EditorNodeStripesProps> = ({ tags }) => {
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

/** The props used for the {@link EditorNodeField} component. */
interface EditorNodeFieldProps {
  /** The unique identifier for the operation field in the workflow. */
  id: string;
  /** The key for the field of the operation. */
  field: string;
  /** The side of the node that this connector should be on. */
  side: "input" | "output";
  /** The schema to render for the field connector. */
  schema: JsonSchema;

  /** An event listener that is called when a field connection point is picked. */
  onPickField?: (field: string, side: "input" | "output") => void;
}
/** A component that renders a particular operation field for a node. */
const EditorNodeField: FC<EditorNodeFieldProps> = ({
  id,
  field,
  side,
  schema,
  onPickField = () => {},
}) => {
  // We create a documentation tooltip for the field.
  const documentation = (
    <ReactMarkdown>{schema.description ?? ""}</ReactMarkdown>
  );

  return (
    <div className={classNames(styles.field, styles[side])}>
      <Arrows.Node
        id={id}
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

      {/* TODO: Implement a debugging view that can display partial results. */}
      {/* We only render the schema for the input parameters. */}
      {side === "input" && <SchemaBaseInput schema={schema} />}
    </div>
  );
};

/** The props for the {@link EditorNode} component. */
interface EditorNodeProps extends ComponentProps<"div"> {
  /** The operation instance to render in the node. */
  operation?: Operation | Error;
  /** The operation type to render. */
  type?: OperationType;

  /** Whether the node is currently selected. */
  selected?: boolean;

  /** The layout of the node. Relevant for updating connections and tooltips. */
  layout?: ITile;

  /** An event listener that is called when the node is offset on the grid. */
  onOffset?: (offset: [number, number]) => void;
  /** An event listener that is called when a field connection point is picked. */
  onPickField?: (field: string, side: "input" | "output") => void;
}
/** A component that renders an operation node in the workflow editor. */
const EditorNode: FC<EditorNodeProps> = ({
  operation,
  type,
  selected,
  layout,
  onOffset = () => {},
  onPickField = () => {},
  className,
  ...props
}) => {
  // If the operation is not loaded yet, render a loading symbol.
  if (!operation || !type) {
    return (
      <div className={classNames(styles.node, styles.loading, className)}>
        <LoadingIcon />
      </div>
    );
  }

  // If the operation has an error, render an error message.
  if (operation instanceof Error) {
    return (
      <div className={classNames(styles.node, className)}>
        <Text color="error">{operation.message}</Text>
      </div>
    );
  }

  // We create a documentation tooltip for the operation.
  const documentation = (
    <ReactMarkdown linkTarget="_blank">{type.description ?? ""}</ReactMarkdown>
  );

  return (
    <div
      className={classNames(
        styles.node,
        { [styles.selected]: selected },
        className
      )}
      {...props}
    >
      {/* Render a header with the operation name and */}
      <Mosaic.Tile.Handle onOffset={onOffset}>
        <Tooltip
          className={styles.header}
          component={documentation}
          options={{ placement: "right" }}
          hover
        >
          <span>{type.display}</span>
          <EditorNodeStripes tags={type.tags} />
        </Tooltip>
      </Mosaic.Tile.Handle>
      <div className={styles.body}>
        {/* Render the input schema. */}
        {operation.schema &&
          Object.entries(operation.schema.inputs).map(([key, val]) => (
            <EditorNodeField
              key={`input-${key}`}
              id={`${operation.id}-input-${key}`}
              field={key}
              side="input"
              schema={val}
              onPickField={onPickField}
            />
          ))}
        {/* Render the output schema. */}
        {operation.schema &&
          Object.entries(operation.schema.outputs).map(([key, val]) => (
            <EditorNodeField
              key={`output-${key}`}
              id={`${operation.id}-output-${key}`}
              field={key}
              side="output"
              schema={val}
              onPickField={onPickField}
            />
          ))}
      </div>
    </div>
  );
};

export default EditorNode;
export type { EditorNodeProps };
