import { FC, useCallback, useState } from "react";
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

  // We store the identifiers of the workflow, operation, and job.
  const [workflowIdStored, setWorkflowIdStored] = useState<string>(workflowId);
  const [operationIdStored, setOperationIdStored] = useState<
    string | undefined
  >(operationId);
  const [jobIdStored, setJobIdStored] = useState<string | undefined>(jobId);

  // Functions used to fetch relevant data.
  const workflowFetch = useCallback(
    () => workflowsAPI.getWorkflow(workflowIdStored),
    [workflowIdStored, workflowsAPI]
  );
  const operationFetch = useCallback(
    () =>
      operationIdStored === undefined
        ? Promise.resolve(null)
        : operationsAPI.getOperation(operationIdStored),
    [operationIdStored, operationsAPI]
  );
  const jobFetch = useCallback(
    () =>
      operationIdStored === undefined || jobIdStored === undefined
        ? Promise.resolve(null)
        : operationsAPI.getOperationJob(operationIdStored, jobIdStored),
    [jobIdStored, operationIdStored, operationsAPI]
  );

  // Start fetching and storing data for the operation, job, and workflow.
  const [workflow, workflowRefresh] = useNestedAsync<
    typeof workflowFetch,
    Workflow
  >(workflowFetch, true, seconds(10));
  const [operation, operationRefresh] = useNestedAsync<
    typeof operationFetch,
    Operation | null
  >(operationFetch, true, seconds(10));
  const [job, jobRefresh] = useNestedAsync<typeof jobFetch, Job | null>(
    jobFetch,
    true,
    seconds(5)
  );

  // Wrap the children of this component in a workflows context.
  return (
    <WorkflowsContext.Provider
      value={{
        workflow,
        operation,
        job,
        workflowRefresh,
        operationRefresh,
        jobRefresh,
        setWorkflow: setWorkflowIdStored,
        setOperation: setOperationIdStored,
        setJob: setJobIdStored,
      }}
    >
      {children}
    </WorkflowsContext.Provider>
  );
};
Workflows.Editor = Editor;

export default Workflows;
