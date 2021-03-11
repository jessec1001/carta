import React, { Component } from "react";

import ApiPathView from "./ApiPathView";

import "./ApiRequestView.css";

export interface ApiRequestViewProps {
  method: string;
  path: string;
}
export interface ApiRequestViewState {
  response?: any;
}

export default class ApiRequestView extends Component<
  ApiRequestViewProps,
  ApiRequestViewState
> {
  static displayName = ApiRequestView.name;

  constructor(props: ApiRequestViewProps) {
    super(props);

    this.state = {};

    this.handleSendRequest = this.handleSendRequest.bind(this);
  }

  handleSendRequest() {
    fetch(this.props.path)
      .then((response) => {
        return response.json();
      })
      .then((data) => {
        this.setState({
          response: data,
        });
      });
  }

  render() {
    const tabSize = 2;
    let response = JSON.stringify(this.state.response, null, tabSize);

    return (
      <div className="api-request">
        <ApiPathView
          method={this.props.method}
          path={this.props.path}
          onClick={this.handleSendRequest}
        />
        {this.state.response !== undefined && (
          <code>
            <pre className="api-response">{response}</pre>
          </code>
        )}
      </div>
    );
  }
}
