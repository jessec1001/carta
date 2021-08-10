import { BlockButton } from "components/buttons";
import { FormGroup } from "components/form";
import { WorkflowIcon } from "components/icons";
import { TextFieldInput } from "components/input";
import { Tab, TabContainer } from "components/tabs";
import { SeparatedText } from "components/text";
import ViewContext from "components/views/ViewContext";
import { WorkspaceContext } from "context";
import { useAPI } from "hooks";
import React, { FunctionComponent, useContext, useState } from "react";

const WorkspaceCreateView: FunctionComponent = () => {
  const { viewId, actions } = useContext(ViewContext);
  const { workspace } = useContext(WorkspaceContext);
  const { workflowAPI, workspaceAPI } = useAPI();

  const [name, setName] = useState<string>("");

  const handleCreate = () => {
    (async () => {
      if (!workspace) return;

      const workflow = await workflowAPI.createWorkflow({
        name: name,
      });
      await workflowAPI.commitWorkflowVersion(workflow.id, "Initial version");
      await workspaceAPI.addWorkspaceWorkflow(workspace.id, workflow.id);
    })();
    actions.removeChildElement(viewId);
  };
  const handleClose = () => {
    actions.removeChildElement(viewId);
  };

  return (
    <TabContainer>
      <Tab
        title={
          <React.Fragment>
            <WorkflowIcon padded /> Create Workflow
          </React.Fragment>
        }
        onClose={handleClose}
        closeable
      >
        <div
          style={{
            width: "100%",
            height: "100%",
            padding: "1rem",
          }}
        >
          <SeparatedText>
            Enter a name for your new workflow below. Your workflow will be
            automatically added to this workspace.
          </SeparatedText>
          <FormGroup density="flow" title="Name">
            <TextFieldInput value={name} onChange={setName} />
          </FormGroup>
          <div className="form-spaced-group">
            <BlockButton color="primary" onClick={handleCreate}>
              Create
            </BlockButton>
            <BlockButton color="secondary" onClick={handleClose}>
              Cancel
            </BlockButton>
          </div>
        </div>
      </Tab>
    </TabContainer>
  );
};

export default WorkspaceCreateView;
