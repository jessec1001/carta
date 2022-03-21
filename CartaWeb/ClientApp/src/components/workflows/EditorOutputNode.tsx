import { FC, useState } from "react";
import { Job } from "library/api";
import EditorNode from "./EditorNode";
import { Text } from "components/text";
import { JsonSchema } from "library/schema";
import styles from "./EditorOutputNode.module.css";
import { Mosaic } from "components/mosaic";
import { LoadingIcon } from "components/icons";
import { GraphVisualizer } from "components/visualizations";

/** The base props for {@link EditorOutputNode}-related components. */
interface EditorOutputBaseProps {
  /** The operation identifier underlying the job. */
  operation: string;
  /** The job that generated the output. */
  job: Job;
  /** The schema of the output of the operation. */
  schema: JsonSchema;
  /** The value for the field. */
  value: any;
  /** The field to render from the job. */
  field: string;
}
const EditorOutputJsonNode: FC<EditorOutputBaseProps> = ({ value }) => {
  //* TODO: Implement as a readonly schema input. *//
  //* <SchemaBaseInput schema={schema} value={value} /> *//
  return (
    <pre className={styles.output}>
      <code>{JSON.stringify(value, null, 2)}</code>
    </pre>
  );
};
const EditorOutputPlotNode: FC<EditorOutputBaseProps> = ({
  operation,
  job,
  value,
  field,
}) => {
  // Fetch the plot type from the value.
  const type: string | undefined | null = value && value.type;

  // We store a reference to the container.
  const [container, setContainer] = useState<HTMLDivElement | null>(null);

  // Based on the plot type, visualize the appropriate component.
  if (type === "graph") {
    return (
      <div ref={setContainer}>
        <GraphVisualizer
          path={`api/operations/${operation}/jobs/${job.id}/${field}`}
          container={container}
        />
      </div>
    );
  }

  return null;
};

/** The props used for the {@link EditorOutputNode} component. */
interface EditorOutputNodeProps {
  /** The operation identifier underlying the job. */
  operation: string;
  /** The job containing the output of the operation. */
  job?: Error | Job | null;
  /** The schema of the output of the operation. */
  schema: JsonSchema;
  /** The field to render from the job. */
  field: string;

  /** An event listener that is called when the node is offset on the grid. */
  onOffset?: (offset: [number, number]) => void;
}
/** A component that renders the output of an operation in a node in the workflow editor. */
const EditorOutputNode: FC<EditorOutputNodeProps> = ({
  operation,
  job,
  field,
  schema,
  onOffset = () => {},
  children,
}) => {
  // If the job is an error, show the error message.
  if (job instanceof Error)
    return (
      <EditorNode>
        <Mosaic.Tile.Handle onOffset={onOffset} className={styles.header}>
          Output <Text color="info">({schema.title})</Text>
        </Mosaic.Tile.Handle>
        <Text color="error">{job.message}</Text>
      </EditorNode>
    );

  // If the job is not specified, show nothing.
  if (!job || job.result === undefined || !(field in job.result))
    return (
      <EditorNode>
        <Mosaic.Tile.Handle onOffset={onOffset} className={styles.header}>
          Output <Text color="info">({schema.title})</Text>
        </Mosaic.Tile.Handle>
        <div>{job === undefined && <LoadingIcon />}</div>
      </EditorNode>
    );

  // Get the type of output to display;
  const outputType: "json" | "plot" = schema["ui:plot"] ? "plot" : "json";

  // Get the output value of the field and render it.
  const value = job.result[field];
  return (
    <EditorNode>
      <Mosaic.Tile.Handle onOffset={onOffset} className={styles.header}>
        Output <Text color="info">({schema.title})</Text>
      </Mosaic.Tile.Handle>
      {outputType === "json" && (
        <EditorOutputJsonNode
          operation={operation}
          job={job}
          schema={schema}
          value={value}
          field={field}
        />
      )}
      {outputType === "plot" && (
        <EditorOutputPlotNode
          operation={operation}
          job={job}
          schema={schema}
          value={value}
          field={field}
        />
      )}
    </EditorNode>
  );
};

export default EditorOutputNode;
export type { EditorOutputNodeProps };
