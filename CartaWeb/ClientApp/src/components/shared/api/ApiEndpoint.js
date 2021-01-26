import React, { Component } from 'react';
import './ApiEndpoint.css';

export class ApiEndpoint extends Component {
    static displayName = ApiEndpoint.name;

    render() {
        return (
            <li className='bg-white endpoint'>
                <p className='endpoint-method text-muted'>{this.props.method}</p>
                <p className='endpoint-path'><code className='text-info'>{this.props.path}</code></p>
            </li>
        );
    }
}