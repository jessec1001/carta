import React, { Component } from "react";
import classNames from "classnames";

import "./ApiPathView.css";

export interface ApiPathViewProps {
  className?: string;

  method: string;
  path: string;

  onClick?: () => void;
}

export default class ApiPathView extends Component<ApiPathViewProps> {
  static displayName = ApiPathView.name;

  render() {
    return (
      <div className={classNames("api-path", this.props.className)}>
        <p
          className={classNames("api-path-method", {
            clickable: !!this.props.onClick
          })}
          onClick={this.props.onClick}
        >
          {this.props.method}
        </p>
        <p className="api-path-url">
          <code className="text-info">{this.props.path}</code>
        </p>
      </div>
    );
  }
}
