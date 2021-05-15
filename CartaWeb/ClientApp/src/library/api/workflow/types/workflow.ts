import Action from "./actors/actions";
import Selector from "./selectors/selectors";

export interface WorkflowOperation {
  name?: string;
  actor: Action;
  selector: Selector;
}
export interface Workflow {
  id?: string;
  name?: string;
  operations: WorkflowOperation[];
}
