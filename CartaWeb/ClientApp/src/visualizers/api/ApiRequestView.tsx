import React, { Component } from "react";

import ApiPathView from "./ApiPathView";

import "./ApiRequestView.css";

export interface ApiRequestViewProps {
  method: string;
  path: string;
  body?: any;
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
    fetch(this.props.path, {
      method: this.props.method,
      body: JSON.stringify(this.props.body),
      headers: {
        "Content-Type": "application/json",
      },
    })
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
    let body = JSON.stringify(this.props.body, null, tabSize);
    let response = JSON.stringify(this.state.response, null, tabSize);

    return (
      <div className="api-request">
        <ApiPathView
          method={this.props.method}
          path={this.props.path}
          onClick={this.handleSendRequest}
        />
        {this.props.body !== undefined && (
          <code className="api-code">
            <pre className="api-type">Body</pre>
            <pre className="api-json">{body}</pre>
          </code>
        )}
        {this.state.response !== undefined && (
          <code className="api-code">
            <pre className="api-type">Response</pre>
            <pre className="api-json">{response}</pre>
          </code>
        )}
      </div>
    );
  }
}
