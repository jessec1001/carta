import React, { Component } from "react";

import ApiVisualizer from "../../visualizers/ApiVisualizer";

export class ApiDocs extends Component {
  static displayName = ApiDocs.name;

  render() {
    return (
      <section>
        <h3>API</h3>
        <p>
          Carta implements a simple backend API. Currently, its main APIs are
          the <em>Data</em> API and <code>Meta</code> API. In the future, there
          will likely be a <em>Workflow</em> API that will manage tools and
          transformations applied to graph data.
        </p>
        <p>
          The <em>Data</em> API is responsible for retrieving a particular
          resource from a source collection of data. It has the following
          parameters:
        </p>
        <ul>
          <li>
            <code>source</code>: Must either by <code>Synthetic</code> or{" "}
            <code>HyperThought</code> (case-insensitive). This represents the
            repository that the resource is located within. Data contained
            within the <code>Synthetic</code> source accept additional arguments
            from the query string that change the random generation of this
            data. For instance:
            <pre>
              <code>
                /api/data/synthetic/RandomFiniteUndirectedGraph?seed=123
              </code>
            </pre>
          </li>
          <li>
            <code>resource</code>: Represents the specific resource located in
            the specified repository. For <code>Synthetic</code> data, this can
            either by <code>RandomFiniteUndirectedGraph</code> or{" "}
            <code>RandomInfiniteDirectedGraph</code> (case-insensitive). For{" "}
            <code>HyperThought</code> data, this must be the primary key of the
            workflow process.
          </li>
        </ul>
        <p>
          The <em>Meta</em> API is responsible for generating information on the
          available Carta API endpoints. Currently, it only provides the path of
          the endpoint and the{" "}
          <a href="https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods">
            HTTP method
          </a>{" "}
          of the discussed APIs.
        </p>

        <h4>Endpoints</h4>
        <ApiVisualizer />
      </section>
    );
  }
}
