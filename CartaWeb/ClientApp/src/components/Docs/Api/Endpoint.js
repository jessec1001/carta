import React, { Component } from 'react';
import './Endpoint.css';

export class Endpoint extends Component {
    static displayName = Endpoint.name;

    render() {
        return (
            <div className='endpoint'>
                <p className='endpoint-method text-muted'>{this.props.method}</p>
                <p className='endpoint-path'><code className='text-info'>{this.props.path}</code></p>
            </div>
        );
    }
}