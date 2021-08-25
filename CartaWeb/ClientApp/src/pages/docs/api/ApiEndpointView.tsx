import React, { Component } from "react";

import { MetaEndpoint } from "library/api/meta";

import ApiParameterView from "./ApiParametersView";
import ApiPathView from "./ApiPathView";
import ApiReturnsView from "./ApiReturnsView";
import ApiRequestCollectionView from "./ApiRequestCollectionView";

import "./ApiEndpointView.css";
import { Text } from "components/text";

export interface ApiEndpointViewProps {
  endpoint: MetaEndpoint;
}

export default class ApiEndpointView extends Component<ApiEndpointViewProps> {
  static displayName = ApiEndpointView.name;

  render() {
    return (
      <li className="api-endpoint">
        <ApiPathView
          className="bottom"
          method={this.props.endpoint.method}
          path={this.props.endpoint.path}
        />
        <div className="api-endpoint-content">
          <Text>{this.props.endpoint.description}</Text>

          {/* We don't need to display the parameters table if there are no parameters. */}
          {this.props.endpoint.parameters.length > 0 && (
            <>
              <h5 className="api-endpoint-content-label">Parameters</h5>
              <ApiParameterView parameters={this.props.endpoint.parameters} />
            </>
          )}

          {/* We don't need to display the returns table if there are no returns. */}
          {this.props.endpoint.returns && (
            <>
              <h5 className="api-endpoint-content-label">Returns</h5>
              <ApiReturnsView returns={this.props.endpoint.returns} />
            </>
          )}

          {/* We don't need to display the requests table if there are no sample requests. */}
          {this.props.endpoint.requests &&
            this.props.endpoint.requests.length > 0 && (
              <>
                <h5 className="api-endpoint-content-label">Example Requests</h5>
                <ApiRequestCollectionView
                  requests={this.props.endpoint.requests}
                  parameters={this.props.endpoint.parameters}
                  method={this.props.endpoint.method}
                  path={this.props.endpoint.path}
                />
              </>
            )}
        </div>
      </li>
    );
  }
}
