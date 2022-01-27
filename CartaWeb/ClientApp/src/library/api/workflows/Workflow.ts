import { Identifiable } from "../base";
import { WorkflowConnection } from "./WorkflowConnection";

/** Represents a type of custom operation with connection transferring data between operation inputs and outputs. */
interface Workflow extends Identifiable {
  /** The name of the workflow. */
  name: string;
  /** The description of the workflow. */
  description: string;

  /** A list of unique identifiers of operations contained in the workflow. */
  operations: string[];
  /** A list of connections between operations contained in the workflow. */
  connections: WorkflowConnection[];
}

export type { Workflow };
