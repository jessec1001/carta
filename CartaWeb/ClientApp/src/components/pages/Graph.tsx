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
    properties: Array<VisProperty>
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
            properties: []
        };

        this.handlePropertiesChanges = this.handlePropertiesChanges.bind(this);
    }

    handlePropertiesChanges(properties: Array<VisProperty>) {
        this.setState({
            properties: properties
        });
    }

    render() {
        return (
            <div className="d-flex page flex-row h-100">
                <div className="d-flex flex-column w-75 h-100">
                    <GraphToolbar className="toolbar" />
                    <GraphExplorer request={this.request} onPropertiesChanged={this.handlePropertiesChanges} />
                </div>
                <div className="w-25 h-100">
                    <HeightScroll className="sidebar">
                        <PropertyList properties={this.state.properties} />
                    </HeightScroll>
                </div>
            </div>
        );
    }
}