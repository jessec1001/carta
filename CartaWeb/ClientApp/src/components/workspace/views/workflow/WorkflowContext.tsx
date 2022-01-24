import { Operation, Job, OperationType, Workflow } from "library/api";
import { useContext } from "react";
import { createContext } from "react";

interface IWorkflowContext {
  workflow: Workflow;
  workflowOperation: Operation;
  operations: Operation[];
  operationTypes: OperationType[];
  input: Record<string, any>;
  output: Record<string, any>;
  file: File | null;
  fileField: string | null;
  setFile: (value: File | null) => void;
  setFileField: (value: string | null) => void;
  jobId: string | null;
  setJobId: (value: string | null) => void;
  autoUpdate: boolean;
  setInputField: (key: string, value: any) => void;
  getOutputField: (key: string) => any;
  setAutoUpdate: (auto: boolean) => void;

  selected: Operation[];
  select: (operations: Operation[]) => void;

  connecting: {
    operation: string;
    field: string;
    side: "input" | "output";
  } | null;
  executeWorkflow: (input: any) => Promise<Job | null>;

  addOperation: (type: string, subtype: string | null) => Promise<Operation>;
  updateOperation: (operationId: string, defaults: any) => Promise<Operation>;
  removeOperation: (operationId: string) => Promise<void>;

  tryConnect: (
    operationId: string,
    field: string,
    side: "input" | "output"
  ) => Promise<void>;
  cancelConnect: () => void;
  addConnection: () => void;
  removeConnection: (connectionId: string) => Promise<void>;
}

const WorkflowContext = createContext<IWorkflowContext | undefined>(undefined);
const useWorkflow = (): IWorkflowContext => {
  const context = useContext(WorkflowContext);
  if (!context)
    throw new Error(
      "Workflow hook can only be used inside of workflow context."
    );

  return context;
};

export default WorkflowContext;
export { useWorkflow };
export type { IWorkflowContext };
