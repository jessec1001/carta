import { BlockButton } from "components/buttons";
import { FormGroup } from "components/form";
import { WorkflowIcon } from "components/icons";
import { TextFieldInput } from "components/input";
import { Tabs } from "components/tabs";
import { Text } from "components/text";
import ViewContext from "components/views/ViewContext";
import { WorkspaceContext } from "context";
import { WorkspaceWorkflow } from "library/api";
import React, { FunctionComponent, useContext, useState } from "react";

const WorkspaceCreateView: FunctionComponent = () => {
  const { viewId, actions } = useContext(ViewContext);
  const { workflows } = useContext(WorkspaceContext);

  const [name, setName] = useState<string>("");

  const handleCreate = () => {
    workflows.CRUD.add({ name: name } as WorkspaceWorkflow);
    actions.removeElement(viewId);
  };
  const handleClose = () => {
    actions.removeElement(viewId);
  };

  return (
    // <Tabs.Tab
    //   id={0}
    //   title={
    //     <React.Fragment>
    //       <WorkflowIcon padded /> Create Workflow
    //     </React.Fragment>
    //   }
    //   onClose={handleClose}
    //   closeable
    // >
    <div
      style={{
        width: "100%",
        height: "100%",
        padding: "1rem",
      }}
    >
      <Text>
        Enter a name for your new workflow below. Your workflow will be
        automatically added to this workspace.
      </Text>
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
    // </Tabs.Tab>
  );
};

export default WorkspaceCreateView;
