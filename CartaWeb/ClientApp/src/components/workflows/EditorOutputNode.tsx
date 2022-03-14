import { FC } from "react";
import { Job } from "library/api";
import EditorNode from "./EditorNode";
import { Text } from "components/text";
import { JsonSchema } from "library/schema";
import styles from "./EditorOutputNode.module.css";
import { Mosaic } from "components/mosaic";
import { LoadingIcon } from "components/icons";
import { SchemaBaseInput } from "components/form/schema";

/** The props used for the {@link EditorOutputNode} component. */
interface EditorOutputNodeProps {
  /** The job containing the output of the operation. */
  job?: Job | null | Error;
  /** The schema of the output of the operation. */
  schema: JsonSchema;
  /** The field to render from the job. */
  field: string;

  /** An event listener that is called when the node is offset on the grid. */
  onOffset?: (offset: [number, number]) => void;
}
/** A component that renders the output of an operation in a node in the workflow editor. */
const EditorOutputNode: FC<EditorOutputNodeProps> = ({
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
  if (!job)
    return (
      <EditorNode>
        <Mosaic.Tile.Handle onOffset={onOffset} className={styles.header}>
          Output <Text color="info">({schema.title})</Text>
        </Mosaic.Tile.Handle>
        <div>{job === undefined && <LoadingIcon />}</div>
      </EditorNode>
    );

  // Get the output value of the field and render it.
  const value = job.result![field];
  return (
    <EditorNode>
      <Mosaic.Tile.Handle onOffset={onOffset} className={styles.header}>
        Output <Text color="info">({schema.title})</Text>
      </Mosaic.Tile.Handle>
      {/* TODO: Implement as a readonly schema input. */}
      {/* <SchemaBaseInput schema={schema} value={value} /> */}
      <pre className={styles.output}>
        <code>{JSON.stringify(value, null, 2)}</code>
      </pre>
    </EditorNode>
  );
};

export default EditorOutputNode;
export type { EditorOutputNodeProps };
