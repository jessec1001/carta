import React, { Component, HTMLProps } from "react";
import {
  Button,
  Nav,
  NavItem,
  NavLink,
  Modal,
  ModalBody,
  ModalFooter,
} from "reactstrap";
import { WorkflowApi } from "lib/api";
import { Workflow } from "../../lib/types/workflow";

export interface WorkflowApplyFormProps extends HTMLProps<HTMLDivElement> {
  open?: boolean;

  onApply?: (workflowId: number) => void;
  onCancel?: () => void;
}
export interface WorkflowApplyFormState {
  open: boolean;
  workflows?: Workflow[];
}

export default class WorkflowApplyForm extends Component<
  WorkflowApplyFormProps,
  WorkflowApplyFormState
> {
  constructor(props: WorkflowApplyFormProps) {
    super(props);

    this.state = {
      open: props.open === true,
    };
    this.populateWorkflows();

    this.handleToggle = this.handleToggle.bind(this);
    this.handleSelect = this.handleSelect.bind(this);
    this.handleCancel = this.handleCancel.bind(this);
  }

  componentDidUpdate(prevProps: WorkflowApplyFormProps) {
    if (this.props.open !== prevProps.open) {
      this.setState({
        open: this.props.open === true,
      });
    }
  }

  handleToggle() {
    this.setState((state) => ({
      open: !state.open,
    }));
  }

  handleSelect(workflowId: number) {
    if (this.props.onApply) this.props.onApply(workflowId);
    this.setState({
      open: false,
    });
  }
  handleCancel() {
    if (this.props.onCancel) this.props.onCancel();
    this.setState({
      open: false,
    });
  }

  async populateWorkflows() {
    this.setState({
      workflows: await WorkflowApi.getWorfklowsAsync(),
    });
  }

  render() {
    const { open, onSelect, onCancel, ...rest } = this.props;
    return (
      <div {...rest}>
        <Modal isOpen={this.state.open} toggle={this.handleToggle}>
          <ModalBody>
            <p>Select a workflow to apply to the current graph.</p>
            {this.state.workflows && (
              <Nav vertical>
                {this.state.workflows.map((workflow) => (
                  <NavItem key={workflow.id}>
                    <NavLink
                      href="#"
                      onClick={() => this.handleSelect(workflow.id as number)}
                    >
                      {workflow.name ?? "(Unnamed)"}
                    </NavLink>
                  </NavItem>
                ))}
              </Nav>
            )}
          </ModalBody>
          <ModalFooter>
            <Button color="secondary" onClick={this.handleCancel}>
              Cancel
            </Button>
          </ModalFooter>
        </Modal>
      </div>
    );
  }
}
