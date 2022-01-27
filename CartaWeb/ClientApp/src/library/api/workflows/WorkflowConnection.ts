import { Identifiable } from "../base";

/** Represents a point that a workflow connection can connect to. */
interface WorkflowConnectionPoint {
  /** The unique identifier of the operation to connect to. */
  operation: string;
  /** The field identifier on the operation to connect to. */
  field: string;
}
/** Represents a connection from a source to a target in a workflow. */
interface WorkflowConnection extends Identifiable {
  /** The source of the connection. */
  source: WorkflowConnectionPoint;
  /** The target of the connection. */
  target: WorkflowConnectionPoint;

  /** Whether the connection should be multiplexed. */
  multiplex: boolean;
}

export type { WorkflowConnection, WorkflowConnectionPoint };
