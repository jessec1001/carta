import React, { Component, SyntheticEvent } from "react";

import {
  Container,
  FormGroup,
  FormText,
  Input,
  InputGroup,
  Button,
} from "reactstrap";

export interface UserPageState {
  hyperthoughtApiKey: string;
}

export default class UserPage extends Component<{}, UserPageState> {
  static displayName = UserPage.name;

  constructor(props: {}) {
    super(props);

    this.state = {
      hyperthoughtApiKey: localStorage.getItem("hyperthoughtKey") ?? "",
    };

    this.handleHyperthoughtApiKeyChanged = this.handleHyperthoughtApiKeyChanged.bind(
      this
    );
    this.handleHyperthoughtApiKeySave = this.handleHyperthoughtApiKeySave.bind(
      this
    );
  }

  handleHyperthoughtApiKeyChanged(event: SyntheticEvent) {
    this.setState({
      hyperthoughtApiKey: (event.target as HTMLInputElement).value,
    });
  }
  handleHyperthoughtApiKeySave() {
    localStorage.setItem("hyperthoughtKey", this.state.hyperthoughtApiKey);
  }

  render() {
    return (
      <Container className="pb-4 mt-4">
        <h2>User</h2>
        <p>
          This page contains information and settings regarding your user
          account. Note that currently, the user account information is only
          stored locally on your own computer. This will change in the future.
        </p>

        <h3>HyperThought&trade; API Authentication</h3>
        <FormGroup>
          <InputGroup>
            <Input
              type="text"
              placeholder="API Access Key"
              value={this.state.hyperthoughtApiKey}
              onChange={this.handleHyperthoughtApiKeyChanged}
            />
            <Button onClick={this.handleHyperthoughtApiKeySave}>Save</Button>
          </InputGroup>
          <FormText color="muted">
            This is the base-64 encoded code which can be retrieved from the
            HyperThought&trade;{" "}
            <a href="https://www.hyperthought.io/api/common/my_account/">
              Account
            </a>{" "}
            page.
          </FormText>
        </FormGroup>
      </Container>
    );
  }
}
