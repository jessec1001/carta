import React, { Component } from "react";
import { Property } from "./Property";
import './PropertyList.css';

export class PropertyList extends Component {
    static displayName = PropertyList.name;

    constructor(props) {
        super(props);

        this.resolveSemantic = this.resolveSemantic.bind(this);
    }

    render() {
        return (
            <div>
                <header className="property-list-header">
                    {this.props.children}
                </header>
                <ul className="property-list">
                {Object.keys(this.props.properties).map(key => {
                        let semanticKey = this.resolveSemantic(key);
                        if (semanticKey) {
                            return (
                                <li key={key}>
                                    <Property
                                        name={semanticKey}
                                        selected={this.props.selected && this.props.selected.includes(key)}
                                        value={this.props.properties[key].value}
                                        occurrences={this.props.properties[key].occurrences}
                                        type={this.props.properties[key].type}
                                        onClick={this.props.onClickProperty ? (event) => this.props.onClickProperty(key, event) : null}
                                    />
                                </li>
                            );
                        }
                        return null;
                    }
                )}
                </ul>
            </div>
        );
    }

    resolveSemantic(key) {
        if (!this.props.semantics)
            return key;

        let semanticName = key;
        while (semanticName in this.props.semantics) {
            if (semanticName === this.props.semantics[semanticName])
                break;
            semanticName = this.props.semantics[semanticName];
        }

        if (key !== semanticName && semanticName in this.props.properties)
            return null;
        return semanticName;
    }
}