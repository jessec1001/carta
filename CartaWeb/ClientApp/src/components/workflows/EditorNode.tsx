import { ComponentProps, FC, useContext } from "react";
import classNames from "classnames";
import { Operation, OperationType } from "library/api";
import { JsonSchema } from "library/schema";
import { Theme, ThemeContext } from "components/theme";
import styles from "./EditorNode.module.css";
import { SchemaBaseInput } from "components/form/schema";

// TODO: Add popper message to hovering over the title of the node.
// TODO: Add popper message to hovering over the tag strips of the node.
//       This popper should contain links for tags that immediately open the operation palette to that tag.
// TODO: Reintegrate logic for visualizing any data that may be associated with the node.
//       This might better be placed in another visualization component.
// TODO: Can we render the output schema of the operation? This would be used when the job is completed or
//       partially completed and we want to visualize the output of intermediate operations.
//       This would particularly be applicable for a debug mode for workflows.
// TODO: Allow for collapsing the node.

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

/** The props used for the {@link EditorNodeConnector} component. */
interface EditorNodeConnectorProps {
  /** The side of the node that this connector should be on. */
  side: "input" | "output";
  /** The schema to render for the field connector. */
  schema: JsonSchema;
}
/** A component that renders a particular operation field for a node. */
const EditorNodeConnector: FC<EditorNodeConnectorProps> = ({
  side,
  schema,
}) => {
  return (
    <div>
      <div />
      <SchemaBaseInput schema={schema} />
    </div>
  );
};

/** The props for the {@link EditorNode} component. */
interface EditorNodeProps extends ComponentProps<"div"> {
  /** The operation instance to render in the node. */
  operation: Operation;
  /** The operation type to render. */
  type: OperationType;

  /** Whether the node is currently selected. */
  selected?: boolean;
}
/** A component that renders an operation node in the workflow editor. */
const EditorNode: FC<EditorNodeProps> = ({
  operation,
  type,
  selected,
  className,
  ...props
}) => {
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
      <div className={styles.header}>
        <span>{type.display}</span>
        <EditorNodeStripes tags={type.tags} />
      </div>
      <div className={styles.body}>
        {/* Render the input schema. */}
        {operation.schema &&
          Object.entries(operation.schema.inputs).map(([key, value]) => (
            <EditorNodeConnector
              key={`input-${key}`}
              side="input"
              schema={value}
            />
          ))}
        {/* Render the output schema. */}
        {operation.schema &&
          Object.entries(operation.schema.outputs).map(([key, value]) => (
            <EditorNodeConnector
              key={`output-${key}`}
              side="output"
              schema={value}
            />
          ))}
      </div>
    </div>
  );
};

export default EditorNode;
export type { EditorNodeProps };
