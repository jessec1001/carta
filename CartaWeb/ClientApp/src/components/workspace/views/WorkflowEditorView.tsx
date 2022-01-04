import { FC, useMemo } from "react";
import { Text } from "components/text";
import { Views } from "components/views";
import { WorkflowIcon } from "components/icons";

interface WorkflowEditorViewProps {
  workflowId: string;
  jobId?: string;
}

const WorkflowEditorView: FC = () => {
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <WorkflowIcon padded /> [Workflow]
      </Text>
    );
  }, []);

  return <Views.Container title={title}></Views.Container>;
};

export default WorkflowEditorView;
export type { WorkflowEditorViewProps };
