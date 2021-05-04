import Action from "./actors/actions";
import Selector from "./selectors/selectors";

export interface WorkflowOperation {
  name?: string;
  action: Action;
  selector: Selector;
}
export interface Workflow {
  id?: number;
  name?: string;
  operations: WorkflowOperation[];
}
