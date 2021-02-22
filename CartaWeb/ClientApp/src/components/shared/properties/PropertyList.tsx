import React, { Component, MouseEvent } from "react";
import { VisProperty } from "../../../lib/types/vis-format";
import { Property } from "./Property";
import './PropertyList.css';

interface PropertyListProps {
    properties: Array<VisProperty>,
    selected?: Array<string>,

    onClickProperty?: (key: string, event: MouseEvent) => void
}

export class PropertyList extends Component<PropertyListProps> {
    static displayName = PropertyList.name;

    render() {
        return (
            <div>
                <ul className="property-list">
                {this.props.properties.map(property => (
                    <li key={property.id}>
                        <Property
                            property={property}
                            selected={this.props.selected && this.props.selected.includes(property.id)}
                            onClick={(event: MouseEvent) => this.props.onClickProperty && this.props.onClickProperty(property.id, event)}
                        />
                    </li>
                ))}
                </ul>
            </div>
        );
    }
}