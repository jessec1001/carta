import { Identifiable } from "../base";

interface WorkflowConnectionPoint {
  operation: string;
  field: string;
}
interface WorkflowConnection extends Identifiable {
  source: WorkflowConnectionPoint;
  target: WorkflowConnectionPoint;

  multiplex: boolean;
}

export default WorkflowConnection;
