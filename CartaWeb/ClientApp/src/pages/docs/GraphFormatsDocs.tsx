import { Component } from "react";
import { ColumnTable } from "components/ui/layout/ColumnTable.js";
import { Subsection } from "components/structure";

export class GraphFormatDocs extends Component {
  static displayName = GraphFormatDocs.name;

  render() {
    return (
      <Subsection title="Graph Formats">
        <p>
          Carta supports a large set of native graph formats to load data into
          its tools. They range across a range of standardized or widely-used
          formats that can be obtained from the <em>Data</em> API and its
          endpoints by setting the appropriate <code>Accept</code> header in API
          calls. By default, the Carta <em>Data</em> API returns a JSON response
          in the "Vis Dataset" format to be easily used by the graphing view.
        </p>

        <h5>JSON-based Formats</h5>
        <p>
          The following formats are JSON-based formats that are useful for being
          loaded directly into JavaScript applications.
        </p>
        <ColumnTable
          headers={["Name", "Site", "MIME"]}
          values={[
            {
              name: "JSON Graph Format (JGF)",
              site: (
                <a href="https://jsongraphformat.info/">
                  {"https://jsongraphformat.info/"}
                </a>
              ),
              mime: <code>application/vnd.jgf+json</code>,
            },
            {
              name: "Vis Dataset",
              site: (
                <a href="https://visjs.github.io/vis-data/data/">
                  {"https://visjs.github.io/vis-data/data/"}
                </a>
              ),
              mime: <code>application/vnd.vis+json</code>,
            },
          ]}
        />

        <h5>XML-based Formats</h5>
        <p>
          The following formats are XML-based formats that are useful for
          applications or languages that natively support XML such as C# or for
          web display.
        </p>
        <ColumnTable
          headers={["Name", "Site", "MIME"]}
          values={[
            {
              name: "Graph Exchange XML Format (GEXF)",
              site: (
                <a href="https://gephi.org/gexf/format/">
                  {"https://gephi.org/gexf/format/"}
                </a>
              ),
              mime: <code>application/vnd.gexf+xml</code>,
            },
            {
              name: "GraphML",
              site: (
                <a href="http://graphml.graphdrawing.org/">
                  {"http://graphml.graphdrawing.org/"}
                </a>
              ),
              mime: <code>application/vnd.graphml+xml</code>,
            },
          ]}
        />

        <h5>Alternative</h5>
        <p>
          The following formats are not represented by any schema for commonly
          used data transfer languages. Instead, they are commonly used formats
          within graph data communities.
        </p>
        <ColumnTable
          headers={["Name", "Site", "MIME"]}
          values={[
            {
              name: "DOT",
              site: (
                <a href="https://graphviz.org/doc/info/lang.html">
                  {"https://graphviz.org/doc/info/lang.html"}
                </a>
              ),
              mime: <code>application/vnd.dot</code>,
            },
          ]}
        />
      </Subsection>
    );
  }
}
