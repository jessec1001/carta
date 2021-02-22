import React, { Component } from "react";
import { VisObservation } from "../../../lib/types/vis-format";
import './ObservationList.css';

interface ObservationListProps {
    observations: Array<VisObservation>
}

export class ObservationList extends Component<ObservationListProps> {
    render() {
        return (
            <ul className='list-unstyled'>
                {this.props.observations.map((obs, index) =>
                    <li key={index} className="observation">
                        <p className="observation-value">{obs.value as any}</p>
                        <p className="observation-type text-muted">{obs.type}</p>
                    </li>
                )}
            </ul>
        );
    }
}