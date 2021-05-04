import React, { Component } from "react";

import MetaApi from "../lib/api/meta/MetaApi";
import { MetaCollection } from "lib/api/meta";

import ApiCollectionView from "./api/ApiCollectionView";

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

    MetaApi.getEndpointsAsync().then((value) => this.setState({ api: value }));
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
