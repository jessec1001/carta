import { BlockButton } from "components/buttons";
import { FormGroup } from "components/form";
import { TextFieldInput } from "components/input";
import { Text } from "components/text";
import { useViews } from "components/views";
import { WorkspaceContext } from "context";
import { WorkspaceWorkflow } from "library/api";
import { FunctionComponent, useContext, useState } from "react";

const WorkspaceCreateView: FunctionComponent = () => {
  const { viewId, actions } = useViews();
  const { workflows } = useContext(WorkspaceContext);

  const [name, setName] = useState<string>("");

  const handleCreate = () => {
    workflows.CRUD.add({ name: name } as WorkspaceWorkflow);
    actions.removeView(viewId);
  };
  const handleClose = () => {
    actions.removeView(viewId);
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
