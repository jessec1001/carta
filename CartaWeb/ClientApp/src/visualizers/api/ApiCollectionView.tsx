import React, { Component } from "react";

import { ApiCollection } from "../../lib/types/meta";

import ApiEndpointView from "./ApiEndpointView";

import "./ApiCollectionView.css";

export interface ApiCollectionViewProps {
  collection: ApiCollection;
}

export default class ApiCollectionView extends Component<ApiCollectionViewProps> {
  static displayName = ApiCollectionView.name;

  render() {
    return (
      <li className="bg-light api-collection">
        <h4 className="api-collection-name">{this.props.collection.name}</h4>
        <p>{this.props.collection.description}</p>
        <ul className="api-endpoint-list">
          {this.props.collection.endpoints.map((endpoint) => (
            <ApiEndpointView key={endpoint.path} endpoint={endpoint} />
          ))}
        </ul>
      </li>
    );
  }
}
