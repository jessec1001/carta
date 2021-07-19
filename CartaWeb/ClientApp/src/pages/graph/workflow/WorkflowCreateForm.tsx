import { TextFieldInput } from "components/input";
import { Component, HTMLProps } from "react";
import { Button, Modal, ModalBody, ModalFooter } from "reactstrap";

export interface WorkflowCreateFormProps extends HTMLProps<HTMLDivElement> {
  open?: boolean;

  onContinue?: (workflow: { name: string }) => void;
  onCancel?: (workflow: null) => void;
}
export interface WorkflowCreateFormState {
  open: boolean;
  name: string;
}

export default class WorkflowCreateForm extends Component<
  WorkflowCreateFormProps,
  WorkflowCreateFormState
> {
  constructor(props: WorkflowCreateFormProps) {
    super(props);

    this.state = {
      open: props.open === true,
      name: "",
    };
    this.handleToggle = this.handleToggle.bind(this);
    this.handleNameUpdate = this.handleNameUpdate.bind(this);
    this.handleContinue = this.handleContinue.bind(this);
    this.handleCancel = this.handleCancel.bind(this);
  }

  componentDidUpdate(prevProps: WorkflowCreateFormProps) {
    if (this.props.open !== prevProps.open) {
      this.setState({
        open: this.props.open === true,
        name: "",
      });
    }
  }

  handleToggle() {
    this.setState((state) => ({
      open: !state.open,
    }));
  }
  handleNameUpdate(value: string) {
    this.setState({
      name: value,
    });
  }

  handleContinue() {
    if (this.props.onContinue) this.props.onContinue({ name: this.state.name });
    this.setState({
      open: false,
    });
  }
  handleCancel() {
    if (this.props.onCancel) this.props.onCancel(null);
    this.setState({
      open: false,
    });
  }

  render() {
    const { open, onContinue, onCancel, ...rest } = this.props;
    return (
      <div {...rest}>
        <Modal isOpen={this.state.open} toggle={this.handleToggle}>
          <ModalBody>
            <p>
              You are creating a new workflow. Please provide a name for the
              workflow.
            </p>
            <TextFieldInput
              placeholder="Workflow Name"
              value={this.state.name}
              onChange={this.handleNameUpdate}
            />
          </ModalBody>
          <ModalFooter>
            <Button color="primary" onClick={this.handleContinue}>
              Create
            </Button>
            <Button color="secondary" onClick={this.handleCancel}>
              Cancel
            </Button>
          </ModalFooter>
        </Modal>
      </div>
    );
  }
}
