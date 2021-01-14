import React, { Component } from "react";

export class Properties extends Component {
    static displayName = Properties.name;

    render() {
        return (
            <div>
                <h2>Properties</h2>
                <ul>
                {Object.keys(this.props.properties).map(key =>
                    <li><strong>{key}: </strong>{this.props.properties[key]}</li>
                )}
                </ul>
            </div>
        );
    }
}