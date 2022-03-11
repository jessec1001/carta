import { createContext, useContext, useMemo } from "react";
import { Job, Operation, Workflow } from "library/api";

/**
 * The type of value used for the {@link WorkflowsContext}.
 * This is meant to provide the minimal functionality to use a workflow.
 * Addition features are defined in {@link IWorkflows}.
 */
interface IWorkflowsContext {
  /** The workflow. If undefined, still loading.*/
  workflow?: Error | Workflow;
  /** The operation associated with the workflow. If undefined, still loading. If null, no operation was specified. */
  operation?: Error | Operation | null;
  /** The job associated with the workflow. If undefined, still loading. If null, no job was specified. */
  job?: Error | Job | null;

  /** A refresh function for the workflow. */
  workflowRefresh: () => Promise<Error | Workflow | undefined>;
  /** A refresh function for the operation associated with the workflow. */
  operationRefresh: () => Promise<Error | Operation | null | undefined>;
  /** A refresh function for the job associated with the workflow. */
  jobRefresh: () => Promise<Error | Job | null | undefined>;

  /** Sets the workflow identifier. */
  setWorkflow: (workflowId: string) => void;
  /** Sets the operation identifier. */
  setOperation: (operationId: string | undefined) => void;
  /** Sets the job identifier. */
  setJob: (jobId: string | undefined) => void;
}

/**
 * Exposes the state of a {@link Workflow}.
 */
interface IWorkflows {
  /** The workflow. If undefined, still loading.*/
  workflow: {
    value?: Error | Workflow;
    set: (workflow: string) => void;
    refresh: () => Promise<Error | Workflow | undefined>;
  };
  /** The operation associated with the workflow. If undefined, still loading. If null, no operation was specified. */
  operation: {
    value?: Error | Operation | null;
    set: (operation: string | undefined) => void;
    refresh: () => Promise<Error | Operation | null | undefined>;
  };
  /** The job associated with the workflow. If undefined, still loading. If null, no job was specified. */
  job: {
    value?: Error | Job | null;
    set: (job: string | undefined) => void;
    refresh: () => Promise<Error | Job | null | undefined>;
  };
}

/** The context used to expose information about the {@link Workflows} component. */
const WorkflowsContext = createContext<IWorkflowsContext | undefined>(
  undefined
);

/**
 * Returns an object that allows for determining the state of a workflow.
 * @returns The state of a wokrflow.
 */
const useWorkflows = (): IWorkflows => {
  // Grab the context if it is defined.
  // If not defined, raise an error because the rest of this hook will not work.
  const context = useContext(WorkflowsContext);
  if (context === undefined) {
    throw new Error(
      "Workflow context must be used within a workflow component."
    );
  }

  // Grab the original functions from the context and reformat them into a more usable format.
  const {
    workflow,
    operation,
    job,
    workflowRefresh,
    operationRefresh,
    jobRefresh,
    setWorkflow,
    setOperation,
    setJob,
  } = context;

  const workflowHelper = useMemo(() => {
    return {
      value: workflow,
      set: setWorkflow,
      refresh: workflowRefresh,
    };
  }, [setWorkflow, workflow, workflowRefresh]);
  const operationHelper = useMemo(() => {
    return {
      value: operation,
      set: setOperation,
      refresh: operationRefresh,
    };
  }, [operation, operationRefresh, setOperation]);
  const jobHelper = useMemo(() => {
    return {
      value: job,
      set: setJob,
      refresh: jobRefresh,
    };
  }, [job, jobRefresh, setJob]);

  return {
    workflow: workflowHelper,
    operation: operationHelper,
    job: jobHelper,
  };
};

export default WorkflowsContext;
export { useWorkflows };
export type { IWorkflowsContext, IWorkflows };
