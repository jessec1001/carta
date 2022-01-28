import { FC, useMemo, useState } from "react";
import { WorkflowIcon } from "components/icons";
import { Text } from "components/text";
import { useViews, Views } from "components/views";
import { FormGroup } from "components/form";
import { Button, ButtonGroup } from "components/buttons";
import { TextFieldInput } from "components/input";

const OperationFromBlankView: FC<{ onSubmit?: (name: string) => void }> = ({
  onSubmit = () => {},
}) => {
  // Store the name of the operation being entered.
  const [name, setName] = useState("");

  // These event handlers are used to either submit or cancel the operation to create.
  const { viewId, actions: viewActions } = useViews();
  const handleCreate = () => {
    onSubmit(name);
    viewActions.removeView(viewId);
  };
  const handleCancel = () => {
    viewActions.removeView(viewId);
  };

  // Create the view title component.
  const title = useMemo(() => {
    return (
      <Text align="middle">
        <WorkflowIcon padded /> Operation from Blank
      </Text>
    );
  }, []);

  return (
    <Views.Container title={title} closeable padded direction="vertical">
      {/* TODO: Check if pressing 'Enter' triggers the submit method. */}
      {/* Render an input for the optional display name of the new operation. */}
      <FormGroup title="Name" density="flow">
        <TextFieldInput placeholder="Unnamed" value={name} onChange={setName} />
      </FormGroup>

      {/* Render a set of buttons to perform or cancel creating the workflow. */}
      <ButtonGroup>
        <Button color="primary" type="submit" onClick={handleCreate}>
          Create
        </Button>
        <Button color="secondary" type="button" onClick={handleCancel}>
          Cancel
        </Button>
      </ButtonGroup>
    </Views.Container>
  );
};

export default OperationFromBlankView;
