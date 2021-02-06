import React, { Component } from 'react';
import { CaretDownFill, CaretUpFill } from 'react-bootstrap-icons';
import { ObservationList } from './ObservationList';
import './Property.css';

export class Property extends Component {
    static displayName = Property.name;

    constructor(props) {
        super(props);

        this.state = {
            expanded: false
        };

        this.handleExpand = this.handleExpand.bind(this);
    }

    handleExpand() {
        this.setState(state => ({
            expanded: !state.expanded
        }));
    }

    render() {
        let className = "property";
        if (this.props.selected)
            className += " selected";
        if (this.props.onClick)
            className += " clickable";

        return (
            <div>
                <div className={className} onClick={this.props.onClick}>
                    <p className="property-name">{this.props.name}:</p>
                    <p className="property-occurrences">
                        ×{this.props.values.length} &nbsp;
                        <div className="d-inline property-expand" onClick={this.handleExpand}>
                            {!this.state.expanded && <CaretDownFill />}
                            {this.state.expanded && <CaretUpFill />}
                        </div>
                    </p>
                </div>
                {this.state.expanded && <ObservationList values={this.props.values} />}
            </div>
        );
    }
}