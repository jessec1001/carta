import React, { Component, MouseEvent } from 'react';
import { CaretDownFill, CaretUpFill } from 'react-bootstrap-icons';
import { VisObservation, VisProperty } from '../../../lib/types/vis-format';
import { ObservationList } from './ObservationList';
import './Property.css';

interface PropertyProps {
    property: VisProperty,
    selected?: boolean,

    onClick?: (event: MouseEvent) => void
}

interface PropertyState {
    expanded: boolean
}

export class Property extends Component<PropertyProps, PropertyState> {
    static displayName = Property.name;

    constructor(props: PropertyProps) {
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
                    <p className="property-name">{this.props.property.id}:</p>
                    <div className="property-occurrences">
                        ×{this.props.property.observations.length} &nbsp;
                        <div className="d-inline property-expand" onClick={this.handleExpand}>
                            {!this.state.expanded && <CaretDownFill />}
                            {this.state.expanded && <CaretUpFill />}
                        </div>
                    </div>
                </div>
                {this.state.expanded && <ObservationList observations={this.props.property.observations} />}
            </div>
        );
    }
}