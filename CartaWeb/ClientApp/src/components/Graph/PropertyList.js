import React, { Component } from "react";
import { Property } from "./Properties/Property";
import './PropertyList.css';

export class PropertyList extends Component {
    static displayName = PropertyList.name;

    render() {
        return (
            <div>
                <h2>Properties</h2>
                <ul className="property-list">
                {Object.keys(this.props.properties).map(key =>
                    <li key={key}>
                        <Property
                            name={key}
                            value={this.props.properties[key].value}
                            type={this.props.properties[key].type}
                        />
                    </li>
                )}
                </ul>
            </div>
        );
    }
}