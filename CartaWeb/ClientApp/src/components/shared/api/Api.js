import React, { Component } from 'react';
import { ApiSection } from './ApiSection';
import './Api.css';

export class Api extends Component {
    static displayName = Api.name;

    constructor(props) {
        super(props);

        this.state = { apis: {}, loading: true };
    }

    componentDidMount() {
        this.populateApis();
    }

    render() {
        return (
            <ul class='api-list'>
                {Object.keys(this.state.apis).map(controller =>
                    <ApiSection key={controller} name={controller} endpoints={this.state.apis[controller]} />
                )}
            </ul>
        );
    }

    async populateApis() {
        const response = await fetch('api/meta');
        const data = await response.json();
        this.setState({ apis: data, loading: false });
    }
}