import React, { Component } from 'react';
import { ApiSection } from './Api/ApiSection';
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
            <section>
                <h3>API</h3>
                <ul class='api-list'>
                    {Object.keys(this.state.apis).map(controller =>
                        <ApiSection key={controller} name={controller} endpoints={this.state.apis[controller]} />
                    )}
                </ul>
            </section>
        );
    }

    async populateApis() {
        const response = await fetch('api/meta');
        const data = await response.json();
        this.setState({ apis: data, loading: false });
    }
}