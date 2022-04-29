// TODO: Things to do for the workflow editor.
/*
 * - Change the way that connection previews work so that they are only rendered
 *   when the mouse is over the appropriate source or target point.
 * - Make the view tab indicate the status of the currently loaded job (error, pending, complete, etc.).
 * - Allow the user to modify the description of the workflow.
 * - Allow for zooming the view in and out.
 * - Allow for easier panning of the view rather than relying on scrolling.
 */

import { FC, useEffect, useMemo, useState } from "react";
import { WorkflowIcon } from "components/icons";
import { Loading, Text } from "components/text";
import { Workflows } from "components/workflows";
import { useViews, Views } from "components/views";
import { Workflow } from "library/api";
import { useAPI } from "hooks";

interface WorkflowEditorViewProps {
  workflowId: string;
  operationId?: string;
  jobId?: string;
}

const WorkflowEditorView: FC<WorkflowEditorViewProps> = ({
  workflowId,
  operationId,
  jobId,
}) => {
  // Get view information.
  const { viewId, actions: viewActions } = useViews();
  const { workflowsAPI } = useAPI();
  const [workflow, setWorkflow] = useState<Workflow | null>(null);
  useEffect(() => {
    const fetchWorkflow = async () => {
      const workflow = await workflowsAPI.getWorkflow(workflowId);
      setWorkflow(workflow);
    };
    fetchWorkflow();
  }, [workflowId, workflowsAPI]);
  console.log(workflow);

  // Create the view title component.
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <WorkflowIcon padded />{" "}
        {workflow ? (
          workflow.name ?? <Text color="muted">(Unnamed)</Text>
        ) : (
          <Loading />
        )}
        &nbsp;
        <Text size="small" color="notify">
          [Editor]
        </Text>
      </Text>
    );
  }, [workflow]);

  // Render the view component.
  return (
    <Views.Container
      title={title}
      closeable
      direction="horizontal"
      onClick={() => viewActions.addHistory(viewId)}
    >
      <Workflows
        workflowId={workflowId}
        operationId={operationId}
        jobId={jobId}
      >
        <Workflows.Editor />
      </Workflows>
    </Views.Container>
  );
};

export default WorkflowEditorView;
export type { WorkflowEditorViewProps };
