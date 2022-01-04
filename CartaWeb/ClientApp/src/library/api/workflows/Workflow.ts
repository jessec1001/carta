import { Identifiable } from "../base";
import WorkflowConnection from "./WorkflowConnection";

interface Workflow extends Identifiable {
  name: string;
  description: string;

  operations: string[];
  connections: WorkflowConnection[];
}

export default Workflow;
