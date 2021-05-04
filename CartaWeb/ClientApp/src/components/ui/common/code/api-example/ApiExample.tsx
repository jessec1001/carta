import React, { Component, HTMLProps } from "react";
import queryString from "query-string";
import { GeneralApi } from "../src2/library/api";
import { CodeExample } from "../code-example";
import "../Code.css";

export interface ApiExampleProps extends HTMLProps<HTMLDivElement> {
  method: string;
  path: string;
  body?: any;
}
export interface ApiExampleState {
  response?: any;
}

export default class ApiExample extends Component<
  ApiExampleProps,
  ApiExampleState
> {
  constructor(props: ApiExampleProps) {
    super(props);
    this.state = {};
    this.handleExecute = this.handleExecute.bind(this);
  }

  handleExecute() {
    const { method, path, body } = this.props;
    const url = queryString.parseUrl(path);

    GeneralApi.requestUnknownAsync(
      url.query,
      { body: body, method: method },
      url.url
    ).then((data: any) => {
      this.setState({
        response: data,
      });
    });
  }

  render() {
    const { method, path, body, ...rest } = this.props;
    const { response } = this.state;

    const tabSize = 2;
    const bodyJson = JSON.stringify(body, null, tabSize);
    const responseJson = JSON.stringify(response, null, tabSize);

    return (
      <div {...rest}>
        <CodeExample
          code={path}
          language="http"
          inline
          copyable
          executable
          onExecute={this.handleExecute}
        />

        {body !== undefined && (
          <section>
            <span className="code-label">Body</span>
            <CodeExample code={bodyJson} language="json" copyable />
          </section>
        )}

        {response !== undefined && (
          <section>
            <span className="code-label">Response</span>
            <CodeExample code={responseJson} language="json" copyable />
          </section>
        )}
      </div>
    );
  }
}
