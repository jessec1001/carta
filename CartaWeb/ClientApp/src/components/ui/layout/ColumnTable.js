import React, { Component } from "react";

export class ColumnTable extends Component {
  static displayName = ColumnTable.name;

  render() {
    return (
      <table>
        <thead>
          <tr className="d-flex">
            {this.props.headers.map((header) => (
              <th key={header} className="col">
                {header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {this.props.values.map((value, index) => (
            <tr key={index} className="d-flex">
              {this.props.headers.map((header) => (
                <td key={header} className="col">
                  {value[header.toLowerCase()]}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    );
  }
}
