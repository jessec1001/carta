import React, { Component } from "react";

export interface ApiReturnsViewProps {
  returns: Record<number, string>;
}

export default class ApiReturnsView extends Component<ApiReturnsViewProps> {
  static displayName = ApiReturnsView.name;

  render() {
    return (
      <table className="api-description-table">
        <thead>
          <tr>
            <th>Status</th>
            <th className="grow">Description</th>
          </tr>
        </thead>
        <tbody>
          {Object.keys(this.props.returns).map((status: string) => (
            <tr key={status}>
              <td>{status}</td>
              <td>{this.props.returns[status as any]}</td>
            </tr>
          ))}
        </tbody>
      </table>
    );
  }
}
