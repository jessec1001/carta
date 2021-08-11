import { WorkflowIcon } from "components/icons";
import { VerticalScroll } from "components/scroll";
import { Tab } from "components/tabs";
import { Heading, LoadingText } from "components/text";
import { ViewContext } from "components/views";
import { WorkspaceContext } from "context";
import { useAPI } from "hooks";
import { WorkflowVersion, WorkspaceWorkflow } from "library/api";
import React, {
  FunctionComponent,
  useContext,
  useEffect,
  useState,
} from "react";

interface WorkflowVersionsViewProps {
  id: string;
}

const WorkflowVersionsView: FunctionComponent<WorkflowVersionsViewProps> = ({
  id,
}) => {
  const { workspaceAPI, workflowAPI } = useAPI();
  const { viewId, actions } = useContext(ViewContext);
  const { workspace } = useContext(WorkspaceContext);

  const [versions, setVersions] = useState<WorkflowVersion[] | null>(null);
  const [workflow, setWorkflow] = useState<WorkspaceWorkflow | null>(null);
  useEffect(() => {
    if (workspace !== null) {
      (async () => {
        console.log(id, workspace);
        const workflow = await workspaceAPI.getWorkspaceWorkflow(
          workspace.id,
          id
        );
        const versions = await workflowAPI.getWorkflowVersions(workflow.id);

        setWorkflow(workflow);
        setVersions(versions);
      })();
    }
  }, [workspace, workspaceAPI, workflowAPI, id]);

  const handleClose = () => {
    actions.removeElement(viewId);
  };

  return (
    <Tab
      title={
        <React.Fragment>
          <WorkflowIcon padded />
          {workflow && workflow.name}
          {!workflow && <LoadingText />}
          &nbsp;
          <span
            style={{
              color: "var(--color-stroke-faint)",
              fontSize: "var(--font-small)",
            }}
          >
            [Versions]
          </span>
        </React.Fragment>
      }
      closeable
      onClose={handleClose}
    >
      <VerticalScroll>
        <div
          style={{
            padding: "1rem",
          }}
        >
          {!versions && <LoadingText />}
          {versions && (
            <ul role="presentation">
              {versions.map((version, index) => {
                return (
                  <li key={version.number}>
                    <div>
                      <p
                        style={{
                          fontSize: "var(--font-medium)",
                        }}
                      >
                        Version {version.number}
                      </p>
                      <p
                        style={{
                          color: "var(--color-stroke-lowlight)",
                        }}
                      >
                        {version.description}
                      </p>
                      <p
                        style={{
                          fontSize: "var(--font-small)",
                          color: "var(--color-stroke-faint)",
                        }}
                      >
                        Created by @{version.createdBy.name} on{" "}
                        <time>{version.dateCreated.toLocaleDateString()}</time>.
                      </p>
                    </div>
                    {index > 0 && <br />}
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      </VerticalScroll>
    </Tab>
  );
};

export default WorkflowVersionsView;
