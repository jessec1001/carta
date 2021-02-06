import React, { Component } from "react";
import { VisProperty } from "../../../lib/types/vis-format";
import './ObservationList.css';

interface ObservationListProps {
    values: Array<VisProperty>
}

export class ObservationList extends Component<ObservationListProps> {
    render() {
        return (
            <ul className='list-unstyled'>
                {this.props.values.map((prop, index) =>
                    <li key={index} className="observation">
                        <p className="observation-value">{prop.value as any}</p>
                        <p className="observation-type text-muted">{prop.type}</p>
                    </li>
                )}
            </ul>
        );
    }
}