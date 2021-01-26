import React, { Component } from 'react';
import Container from 'reactstrap/lib/Container';
import { Api } from '../shared/api/Api';

export class Docs extends Component {
    static displayName = Docs.name;

    render() {
        return (
            <Container>
                <h2>Documentation</h2>
                <Api />
            </Container>
        );
    }
}