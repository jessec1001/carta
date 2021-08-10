/** Represents a workflow operation. */
interface WorkflowOperation {
  // TODO: Create a type for actor.
  /** The actor of the operation. */
  actor: any;
  // TODO: Create a type for selector.
  /** The selector of the operation. */
  selector: any;
}

export type { WorkflowOperation };
