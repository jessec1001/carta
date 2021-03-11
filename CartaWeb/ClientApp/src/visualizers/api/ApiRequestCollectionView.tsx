import React, { Component } from "react";
import queryString from "query-string";

import { ApiParameter, ApiRequest } from "../../lib/types/meta";

import { TabPane, Tab } from "../../ui/panes";
import ApiRequestView from "./ApiRequestView";

import "./ApiRequestCollectionView.css";

export interface ApiRequestCollecetionViewProps {
  method: string;
  path: string;
  requests: ApiRequest[];
  parameters: ApiParameter[];
}

export default class ApiRequestCollectionView extends Component<ApiRequestCollecetionViewProps> {
  static displayName = ApiRequestCollectionView.name;

  resolvePath(path: string, request: ApiRequest) {
    this.props.parameters.forEach((param) => {
      if (param.name in request.arguments) {
        if (param.format === "route") {
          path = path.replace(`{${param.name}}`, request.arguments[param.name]);
        } else if (param.format === "query") {
          const url = queryString.parseUrl(path);
          path = queryString.stringifyUrl({
            url: url.url,
            query: {
              ...url.query,
              [param.name]: request.arguments[param.name],
            },
          });
        }

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
            ...extraQuery
          },
        });
      }
    });
    return path;
  }

  render() {
    return (
      <TabPane className="api-requests">
        {this.props.requests.map((request: ApiRequest) => (
          <Tab label={request.name}>
            <ApiRequestView
              method={this.props.method}
              path={this.resolvePath(this.props.path, request)}
            />
          </Tab>
        ))}
      </TabPane>
    );
  }
}
