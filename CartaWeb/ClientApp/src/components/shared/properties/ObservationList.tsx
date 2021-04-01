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
            <p className="observation-value">{obs}</p>
            <p className="observation-type text-muted">{typeof obs}</p>
          </li>
        ))}
      </ul>
    );
  }
}
