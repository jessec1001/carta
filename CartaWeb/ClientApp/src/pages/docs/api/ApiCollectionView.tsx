import React, { Component } from "react";

import { MetaCollection } from "library/api/meta";

import ApiEndpointView from "./ApiEndpointView";

import "./ApiCollectionView.css";
import { SeparatedText } from "components/text";

export interface ApiCollectionViewProps {
  collection: MetaCollection;
}

export default class ApiCollectionView extends Component<ApiCollectionViewProps> {
  static displayName = ApiCollectionView.name;

  render() {
    return (
      <li className="bg-light api-collection">
        <h4 className="api-collection-name">{this.props.collection.name}</h4>
        <SeparatedText>{this.props.collection.description}</SeparatedText>
        <ul className="api-endpoint-list">
          {this.props.collection.endpoints.map((endpoint) => (
            <ApiEndpointView
              key={`${endpoint.method} ${endpoint.path}`}
              endpoint={endpoint}
            />
          ))}
        </ul>
      </li>
    );
  }
}
