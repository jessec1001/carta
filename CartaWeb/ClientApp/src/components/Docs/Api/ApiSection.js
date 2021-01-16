import React, { Component } from 'react';
import { ApiEndpoint } from './ApiEndpoint';
import './ApiSection.css';

export class ApiSection extends Component {
    render() {
        return (
            <li className='bg-light api-section'>
                <h4 className='api-section-name'>{this.props.name}</h4>
                <ul className='endpoint-list'>
                    {this.props.endpoints.map((endpoint, index) => 
                        <ApiEndpoint key={index} method={endpoint.method} path={endpoint.path} />
                    )}
                </ul>
            </li>
        );
    }
}