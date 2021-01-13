import React, { Component } from "react";

export class GraphDisplayProperties extends Component {
    static displayName = GraphDisplayProperties.name;

    render() {
        return (
            <div>
                <h2>Properties</h2>
                <ul>
                {Object.keys(this.props.properties).map((key, index) =>
                    <li><strong>{key}: </strong>{this.props.properties[key]}</li>
                )}
                </ul>
            </div>
        );
    }
}