import { Operation, Job, OperationType, Workflow } from "library/api";
import { useContext } from "react";
import { createContext } from "react";

interface IWorkflowContext {
  workflow: Workflow; // Transferred.
  workflowOperation: Operation; // Transferred.
  operations: Operation[]; // TODO
  operationTypes: OperationType[]; // TODO
  input: Record<string, any>; // Necessary or part of workflow operation job?
  output: Record<string, any>; // Necessary or part of workflow operation job?
  file: File | null; // TODO
  fileField: string | null; // TODO
  setFile: (value: File | null) => void; // TODO
  setFileField: (value: string | null) => void; // TODO
  jobId: string | null; // Transferred.
  setJobId: (value: string | null) => void; // Transferred.
  autoUpdate: boolean; // TODO
  setInputField: (key: string, value: any) => void; // See above.
  getOutputField: (key: string) => any; // See above.
  setAutoUpdate: (auto: boolean) => void; // TODO

  selected: Operation[]; // TODO
  select: (operations: Operation[]) => void; // TODO

  connecting: {
    operation: string;
    field: string;
    side: "input" | "output";
  } | null; // TODO
  executeWorkflow: (input: any) => Promise<Job | null>; // TODO

  addOperation: (type: string, subtype: string | null) => Promise<Operation>; // TODO
  updateOperation: (operationId: string, defaults: any) => Promise<Operation>; // TODO
  removeOperation: (operationId: string) => Promise<void>; // TODO

  tryConnect: (
    operationId: string,
    field: string,
    side: "input" | "output"
  ) => Promise<void>; // TODO
  cancelConnect: () => void; // TODO
  addConnection: () => void; // TODO
  removeConnection: (connectionId: string) => Promise<void>; // TODO
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
