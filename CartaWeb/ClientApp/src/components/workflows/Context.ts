import { Job, Operation, Workflow } from "library/api";
import { createContext, useContext } from "react";

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
}

/**
 * Exposes the state of a {@link Workflow}.
 */
interface IWorkflows {
  /** The workflow. If undefined, still loading.*/
  workflow?: Error | Workflow;
  /** The operation associated with the workflow. If undefined, still loading. If null, no operation was specified. */
  operation?: Error | Operation | null;
  /** The job associated with the workflow. If undefined, still loading. If null, no job was specified. */
  job?: Error | Job | null;
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
  return { ...context };
};

export default WorkflowsContext;
export { useWorkflows };
export type { IWorkflowsContext, IWorkflows };
