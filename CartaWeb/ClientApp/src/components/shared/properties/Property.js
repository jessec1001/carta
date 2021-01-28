import React, { Component } from 'react';
import './Property.css';

export class Property extends Component {
    static displayName = Property.name;

    render() {
        let name = this.props.name;
        let value = this.props.value;
        let type = this.props.type;
        let occurrences = this.props.occurrences;
        
        // Round to 6 sig figs if a floating point number.
        if (value !== undefined && (type === 'double' || type === 'float'))
            value = value.toPrecision(6);

        let className = "property";
        if (this.props.selected)
            className += " selected";
        if (this.props.onClick)
            className += " clickable";

        return (
            <div className={className} onClick={this.props.onClick}>
                <p className="property-name">{name}</p>
                {!occurrences && <p className="property-value">{value}</p>}
                {occurrences && <p className="property-occurrences">×{occurrences}</p>}
                <p className="property-type text-muted">{type}</p>
            </div>
        );
    }
}