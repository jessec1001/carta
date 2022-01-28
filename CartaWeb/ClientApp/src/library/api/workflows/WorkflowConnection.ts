import { Identifiable } from "../base";

/**
 * Represents a point that a workflow connection can connect to.
 * @template TId The type of the unique identifier for operations.
 */
interface WorkflowConnectionPoint<TId = string> {
  /** The unique identifier of the operation to connect to. */
  operation: TId;
  /** The field identifier on the operation to connect to. */
  field: string;
}
/**
 * Represents a connection from a source to a target in a workflow.
 * @template TId The type of the unique identifier for operations.
 */
interface WorkflowConnection<TId = string> extends Identifiable {
  /** The source of the connection. */
  source: WorkflowConnectionPoint<TId>;
  /** The target of the connection. */
  target: WorkflowConnectionPoint<TId>;

  /** Whether the connection should be multiplexed. */
  multiplex: boolean;
}

export type { WorkflowConnection, WorkflowConnectionPoint };
