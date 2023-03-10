import React, { Component } from "react";
import queryString from "query-string";
import { MetaParameter, MetaRequest } from "library/api/meta";
import { Tabs } from "components/tabs";
import { ApiExample } from "components/common/code";
import "./ApiRequestCollectionView.css";

export interface ApiRequestCollecetionViewProps {
  method: string;
  path: string;
  requests: MetaRequest[];
  parameters: MetaParameter[];
}

export default class ApiRequestCollectionView extends Component<ApiRequestCollecetionViewProps> {
  static displayName = ApiRequestCollectionView.name;

  resolvePath(path: string, request: MetaRequest) {
    this.props.parameters.forEach((param) => {
      if (param.name in request.arguments) {
        const name = param.name;
        if (param.format === "route") {
          path = path.replace(`{${name}?}`, request.arguments[name]);
          path = path.replace(`{${name}}`, request.arguments[name]);
        } else if (param.format === "query") {
          const url = queryString.parseUrl(path);
          path = queryString.stringifyUrl({
            url: url.url,
            query: {
              ...url.query,
              [name]: request.arguments[name],
            },
          });
        }
      }
    });

    path = path.replace(/\{.*?\?\}/g, "");

    const knownArgs = this.props.parameters.map((param) => param.name);
    const extraArgs = Object.keys(request.arguments).filter(
      (arg) => !knownArgs.includes(arg)
    );
    const extraQuery = extraArgs.reduce((obj, key) => {
      return { ...obj, [key]: request.arguments[key] };
    }, {});
    const url = queryString.parseUrl(path);
    path = queryString.stringifyUrl({
      url: url.url,
      query: {
        ...url.query,
        ...extraQuery,
      },
    });

    return path;
  }

  render() {
    return (
      <Tabs>
        {this.props.requests.map((request: MetaRequest) => (
          <Tabs.Tab id={0} key={request.name}>
            <ApiExample
              method={this.props.method}
              path={this.resolvePath(this.props.path, request)}
              body={request.body}
            />
          </Tabs.Tab>
        ))}
      </Tabs>
    );
  }
}
