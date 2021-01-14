import React, { Component } from 'react';
import './Property.css';

export class Property extends Component {
    static displayName = Property.name;

    render() {
        let name = this.props.name;
        let value = this.props.value;
        let type = typeof(value);
        
        // Round to 6 sig figs if a floating point number.
        if (type === 'number' && value % 1 !== 0)
            value = value.toPrecision(6);

        return (
            <div className="property">
                <p className="property-name">{name}</p>
                <p className="property-value">{value}</p>
                <p className="property-type text-muted">{type}</p>
            </div>
        );
    }
}