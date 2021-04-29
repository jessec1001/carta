import React, { Component } from "react";

import { MetaParameter } from "lib/api/meta";

export interface ApiParameterViewProps {
  parameters: MetaParameter[];
}

export default class ApiParameterView extends Component<ApiParameterViewProps> {
  static displayName = ApiParameterView.name;

  render() {
    return (
      <table className="api-description-table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Type</th>
            <th>Format</th>
            <th className="grow">Description</th>
          </tr>
        </thead>
        <tbody>
          {this.props.parameters.map((parameter) => (
            <tr key={parameter.name}>
              <td>{parameter.name}</td>
              <td>
                <code>{parameter.type}</code>
              </td>
              <td>{parameter.format}</td>
              <td>{parameter.description}</td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  }
}
