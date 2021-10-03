import { BlockButton, ButtonGroup } from "components/buttons";
import { FormGroup } from "components/form";
import { WorkflowIcon } from "components/icons";
import { TextAreaInput } from "components/input";
import { VerticalScroll } from "components/scroll";
import { Tabs } from "components/tabs";
import { Loading } from "components/text";
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
  const { workspace, workflows } = useContext(WorkspaceContext);

  const [versions, setVersions] = useState<WorkflowVersion[] | null>(null);
  const [workflow, setWorkflow] = useState<WorkspaceWorkflow | null>(null);
  useEffect(() => {
    if (workspace !== null) {
      (async () => {
        const workflow = await workspaceAPI.getWorkspaceWorkflow(
          workspace.id,
          id
        );
        setWorkflow(workflow);

        const versions = await workflowAPI.getWorkflowVersions(workflow.id);
        setVersions(versions);
      })();
    }
  }, [workspace, workspaceAPI, workflowAPI, id]);

  // TODO: Use schema form to ensure that the description is non-empty.
  const [description, setDescription] = useState<string>("");
  const handleSubmit = (event: React.FormEvent) => {
    if (workflow) {
      workflowAPI.commitWorkflowVersion(workflow.id, description);
      workflows.CRUD.update(workflow).then(setWorkflow);
      workflowAPI.getWorkflowVersions(workflow.id).then(setVersions);
    }

    event.preventDefault();
  };

  const handleClose = () => {
    actions.removeElement(viewId);
  };

  return (
    // <Tabs.Tab
    //   id={0}
    //   title={
    //     <React.Fragment>
    //       <WorkflowIcon padded />
    //       {workflow && workflow.name}
    //       {!workflow && <Loading />}
    //       &nbsp;
    //       <span
    //         style={{
    //           color: "var(--color-stroke-muted)",
    //           fontSize: "var(--font-small)",
    //         }}
    //       >
    //         [Versions]
    //       </span>
    //     </React.Fragment>
    //   }
    //   closeable
    //   onClose={handleClose}
    // >
    <VerticalScroll>
      <div
        style={{
          padding: "1rem",
        }}
      >
        {!versions && <Loading />}
        <form onSubmit={handleSubmit}>
          New Version
          <FormGroup title="Description" density="sparse">
            <TextAreaInput value={description} onChange={setDescription} />
          </FormGroup>
          <ButtonGroup>
            <BlockButton type="submit" color="primary">
              Commit
            </BlockButton>
          </ButtonGroup>
        </form>

        <div style={{ height: "1rem" }} />

        {versions && (
          <React.Fragment>
            Versions
            <ul>
              {versions.map((version, index) => {
                return (
                  <li
                    key={version.number}
                    style={{
                      marginBottom: "0.5rem",
                    }}
                  >
                    <div>
                      <p
                        style={{
                          fontSize: "1.2em",
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
                          color: "var(--color-stroke-muted)",
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
          </React.Fragment>
        )}
      </div>
    </VerticalScroll>
    // </Tabs.Tab>
  );
};

export default WorkflowVersionsView;
