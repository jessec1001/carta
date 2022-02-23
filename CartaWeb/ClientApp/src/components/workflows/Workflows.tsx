import { FC, useCallback } from "react";
import { useAPI, useNestedAsync } from "hooks";
import { Job, Operation, Workflow } from "library/api";
import { seconds } from "library/utility";
import WorkflowsContext from "./Context";
import Editor from "./Editor";

/** The props used for the {@link Workflows} component. */
interface WorkflowsProps {
  /** The unique identifier of the workflow. */
  workflowId: string;
  /** The unique identifier of an operation instantiated from the workflow. Optional. */
  operationId?: string;
  /** The unique identifier of an operation job executed from the workflow. Optional.  */
  jobId?: string;
}
/**
 * Defines the composition of the compound {@link Workflows} component.
 * @borrows Editor as Editor
 */
interface WorkflowsComposition {
  Editor: typeof Editor;
}
/** A component that makes a particular workflow available to subcomponents. */
const Workflows: FC<WorkflowsProps> & WorkflowsComposition = ({
  workflowId,
  operationId,
  jobId,
  children,
}) => {
  // We need access to the API to get the operation, job, and workflow.
  const { workflowsAPI, operationsAPI } = useAPI();

  // Functions used to fetch relevant data.
  const workflowFetch = useCallback(
    () => workflowsAPI.getWorkflow(workflowId),
    [workflowId, workflowsAPI]
  );
  const operationFetch = useCallback(
    () =>
      operationId === undefined
        ? Promise.resolve(null)
        : operationsAPI.getOperation(operationId),
    [operationId, operationsAPI]
  );
  const jobFetch = useCallback(
    () =>
      operationId === undefined || jobId === undefined
        ? Promise.resolve(null)
        : operationsAPI.getOperationJob(operationId, jobId),
    [jobId, operationId, operationsAPI]
  );

  // Start fetching and storing data for the operation, job, and workflow.
  const [workflow] = useNestedAsync<typeof workflowFetch, Workflow>(
    workflowFetch,
    true,
    seconds(10)
  );
  const [operation] = useNestedAsync<typeof operationFetch, Operation | null>(
    operationFetch,
    true,
    seconds(10)
  );
  const [job] = useNestedAsync<typeof jobFetch, Job | null>(
    jobFetch,
    true,
    seconds(10)
  );

  // Wrap the children of this component in a workflows context.
  return (
    <WorkflowsContext.Provider value={{ workflow, operation, job }}>
      {children}
    </WorkflowsContext.Provider>
  );
};
Workflows.Editor = Editor;

export default Workflows;
