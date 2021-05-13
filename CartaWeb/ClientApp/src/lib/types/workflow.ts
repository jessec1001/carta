import Action from "./actions";
import Selector from "./selectors";

export interface WorkflowOperation {
  name?: string;
  actor: Action;
  selector: Selector;
}
export interface Workflow {
  id?: number;
  name?: string;
  operations: WorkflowOperation[];
}
