import React, { Component } from 'react';
import Container from 'reactstrap/lib/Container';
import { Endpoint } from './Api/Endpoint';
import './Api.css';

export class Api extends Component {
    static displayName = Api.name;

    constructor(props) {
        super(props);

        this.apis = [
            {
                method: 'GET',
                path: '/api/data/synthetic:RandomUndirectedGraph'
            },
            {
                method: 'GET',
                path: '/api/graph/synthetic:RandomUndirectedGraph'
            }
        ];
    }

    render() {
        return (
            <section>
                <h3>API</h3>
                <ul className="api-list">
                    {this.apis.map((api, index) =>
                        <Endpoint key={index} method={api.method} path={api.path} />   
                    )}
                </ul>
            </section>
        );
    }
}