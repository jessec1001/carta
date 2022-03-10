import React, { Component } from "react";

import { MetaCollection } from "library/api/meta";

import ApiCollectionView from "./ApiCollectionView";

import "./ApiVisualizer.css";

export interface ApiVisualizerState {
  api: MetaCollection[];
}

export default class ApiVisualizer extends Component<{}, ApiVisualizerState> {
  static displayName = ApiVisualizer.name;

  constructor(props: {}) {
    super(props);

    this.state = {
      api: [],
    };

    // MetaApi.getEndpointsAsync().then((value) => this.setState({ api: value }));
  }

  render() {
    return (
      <ul className="api-collection-list">
        {this.state.api.map((apiCollection) => (
          <ApiCollectionView
            key={apiCollection.name}
            collection={apiCollection}
          />
        ))}
      </ul>
    );
  }
}
