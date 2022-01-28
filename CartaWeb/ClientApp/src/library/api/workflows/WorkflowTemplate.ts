import { Operation } from "../operations";
import { WorkflowConnection } from "../workflows";

/**
 * Represents all of the necessary information to construct a workflow made up of zero or more preconfigured
 * operations.
 */
interface WorkflowTemplate {
  /** A list of operations with types and defaults specified to construct a workflow from. */
  operations: Operation[];
  /** A list of connections with identifiers equal to indices of operations in {@link operations}. */
  connections: WorkflowConnection[];
}

export type { WorkflowTemplate };
