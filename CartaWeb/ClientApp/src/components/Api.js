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
                path: '/api/data/synthetic/randomGraph'
            },
            {
                method: 'GET',
                path: '/api/graph/synthetic/randomGraph'
            }
        ];
    }

    render() {
        return (
            <Container>
                <h2>API</h2>
                <ul className="api-list">
                    {this.apis.map(api =>
                        <Endpoint method={api.method} path={api.path} />   
                    )}
                </ul>
            </Container>
        );
    }
}