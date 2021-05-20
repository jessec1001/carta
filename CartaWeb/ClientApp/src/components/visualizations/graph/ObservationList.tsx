import React, { Component } from "react";
import "./ObservationList.css";

interface ObservationListProps {
  observations: Array<any>;
}

export class ObservationList extends Component<ObservationListProps> {
  render() {
    return (
      <ul className="list-unstyled">
        {this.props.observations.map((obs, index) => (
          <li key={index} className="observation">
            <span className="observation-value">
              {this.renderObservation(obs)}
            </span>
            <span className="observation-type text-muted">
              {this.computeTypeName(obs)}
            </span>
          </li>
        ))}
      </ul>
    );
  }

  computeTypeName(obs: any) {
    if (obs === null) {
      return "null";
    } else if (Array.isArray(obs)) {
      return "array";
    } else if (typeof obs === "object") {
      return "map";
    } else {
      return typeof obs;
    }
  }

  renderNull() {
    return <span style={{ color: "#ff6666" }}>NULL</span>;
  }
  renderEmpty() {
    return <span style={{ color: "#aaaaaa" }}>EMPTY</span>;
  }
  renderObservation(obs: any) {
    if (obs === null) {
      return this.renderNull();
    } else if (Array.isArray(obs)) {
      if (obs.length === 0) return this.renderEmpty();
      else
        return (
          <table>
            <tr>
              {obs.map((value, index) => (
                <td key={index}>{this.renderObservation(value)}</td>
              ))}
            </tr>
          </table>
        );
    } else if (typeof obs === "object") {
      if (Object.keys(obs).length === 0) return this.renderEmpty();
      else
        return (
          <table>
            {Object.entries(obs).map(([key, value]) => (
              <tr key={key}>
                <td>{key}</td>
                <td>{this.renderObservation(value)}</td>
              </tr>
            ))}
          </table>
        );
    } else if (typeof obs === "string" && obs === "") {
      return <span style={{ color: "#aaaaaa" }}>EMPTY</span>;
    } else {
      return obs.toString();
    }
  }
}
