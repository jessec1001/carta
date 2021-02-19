import React, { Component } from 'react';
import { RouteComponentProps } from "react-router-dom";
import { HeightScroll } from '../layouts/HeightScroll';
import { PropertyList } from "../shared/properties/PropertyList";
import { GraphExplorer } from '../forms/GraphExplorer';
import { GraphToolbar } from '../shared/nav/GraphToolbar';
import { VisProperty } from '../../lib/types/vis-format';
import './Graph.css';

interface GraphProps extends RouteComponentProps { }
interface GraphState {
    properties: Array<VisProperty>,
    selection: Array<string>
}

export class Graph extends Component<GraphProps, GraphState> {
    request: string;

    constructor(props : GraphProps) {
        super(props);
        
        this.request = (
            this.props.location.pathname +
            this.props.location.search
        ).replace(this.props.match.path, '').substring(1);
        this.state = {
            properties: [],
            selection: []
        };

        this.handlePropertiesChanges = this.handlePropertiesChanges.bind(this);
        this.handleSelect = this.handleSelect.bind(this);
        this.handleSelection = this.handleSelection.bind(this);
    }

    handlePropertiesChanges(properties: Array<VisProperty>) {
        this.setState({
            properties: properties
        });
    }
    handleSelect(selector: any) {
        this.getSelection(selector);
    }
    handleSelection(selection: Array<string>) {
        this.setState({
            selection: selection
        });
    }

    render() {
        return (
            <div className="d-flex page flex-row h-100">
                <div className="d-flex flex-column w-75 h-100">
                    <GraphToolbar className="toolbar" onSelect={this.handleSelect} />
                    <GraphExplorer
                        request={this.request}
                        selection={this.state.selection}
                        
                        onPropertiesChanged={this.handlePropertiesChanges}
                        onSelection={this.handleSelection}
                    />
                </div>
                <div className="w-25 h-100">
                    <HeightScroll className="sidebar">
                        <PropertyList properties={this.state.properties} />
                    </HeightScroll>
                </div>
            </div>
        );
    }

    appendRoute(url: string, route: string) {
        // Get the location to place to insert the route.
        let searchIndex = url.indexOf('?');
        searchIndex = searchIndex < 0 ? url.length : searchIndex;

        return `${url.substring(0, searchIndex)}/${route}${url.substring(searchIndex)}`;
    }

    async getSelection(selector: any) {
        // Set the body of the request.
        let body: any = {};
        if (this.state.selection.length > 0) {
            body.ids = this.state.selection;
        }
        body.selectors = [ selector ];

        // Grab the data from the server.
        const route = this.appendRoute(this.request, 'select');
        const response = await fetch(`api/data/${route}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(body)
        });
        const selection = await response.json();

        this.setState({
            selection: selection
        });
    }
}