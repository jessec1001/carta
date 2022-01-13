import { FC, useEffect, useMemo, useState } from "react";
import { Text } from "components/text";
import { Views } from "components/views";
import { WorkflowIcon } from "components/icons";
import { Job, Operation, Workflow } from "library/api";
import WorkflowEditor from "./workflow/WorkflowEditor";
import WorkflowWrapper from "./workflow/WorkflowWrapper";
import { useAPI } from "hooks";

interface WorkflowEditorViewProps {
  operation: Operation;
  workflowId: string;
  jobId?: string;
}

const WorkflowEditorView: FC<WorkflowEditorViewProps> = ({
  operation,
  workflowId,
  jobId,
}) => {
  const [freshJob, setFreshJob] = useState<Job | null>(null);
  const [freshWorkflow, setFreshWorkflow] = useState<Workflow | null>(null);
  const [freshOperation, setFreshOperation] = useState<Operation | null>(
    operation
  );

  const { workflowsAPI, operationsAPI } = useAPI();

  useEffect(() => {
    const fetchWorkflow = async () => {
      const workflow = await workflowsAPI.getWorkflow(workflowId);
      setFreshWorkflow(workflow);
    };
    const fetchJob = async () => {
      if (!jobId) return;
      const job = await operationsAPI.getOperationJob(operation.id, jobId);
      setFreshJob(job);
    };
    fetchWorkflow();
    fetchJob();
  }, [jobId, operation, workflowId, operationsAPI, workflowsAPI]);

  const title = useMemo(() => {
    return (
      <Text align="middle">
        <WorkflowIcon padded />{" "}
        {operation.name ?? <Text color="muted">(Unnamed)</Text>}
        &nbsp;
        <Text size="small" color="notify">
          [Workflow]
        </Text>
      </Text>
    );
  }, [operation]);

  return (
    <Views.Container title={title}>
      {freshWorkflow && freshOperation && (
        <WorkflowWrapper
          job={freshJob === null ? undefined : freshJob}
          workflow={freshWorkflow}
          operation={freshOperation}
          onJobUpdate={setFreshJob}
          onWorkflowUpdate={setFreshWorkflow}
          onOperationUpdate={setFreshOperation}
        >
          <WorkflowEditor />
        </WorkflowWrapper>
      )}
    </Views.Container>
  );
};

export default WorkflowEditorView;
export type { WorkflowEditorViewProps };
