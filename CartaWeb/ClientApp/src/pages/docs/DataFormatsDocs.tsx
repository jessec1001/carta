import { Paragraph, Section, Subsection } from "components/text";
import { Component } from "react";
import { GraphFormatDocs } from "./GraphFormatsDocs";

export class DataFormatsDocs extends Component {
  static displayName = DataFormatsDocs.name;

  render() {
    return (
      <Section title="Data Formats">
        <Paragraph>
          Carta provides serialization and deserialization for a variety of
          commonly-used data formats in addition to integration with external
          data sources. This section details which formats are supported, and
          how to access them.
        </Paragraph>

        <Subsection title="Internal">
          <Paragraph>
            Carta uses an intermediary format to store graph data called the{" "}
            <em>Freeform Graph</em>. The data structure is simply and easily
            extensible. Any data that can described using the following
            attributes can be represented by the Carta infrastructure. A{" "}
            <em>Freeform Graph</em> has the following properties.
          </Paragraph>

          <Paragraph>For vertices:</Paragraph>
          <ul>
            <li>
              <code>id</code>: A required unique identifier for each vertex.
            </li>
            <li>
              <code>title</code>: An optional name for the vertex that is
              displayed in the graphin tool.
            </li>
            <li>
              <code>description</code>: An optional description for the vertex
              that is displayed in the graphin tool.
            </li>
            <li>
              <code>properties</code>: A required map between property names and
              type-value pairs.
            </li>
          </ul>

          <Paragraph>For edges:</Paragraph>
          <ul>
            <li>
              <code>source</code>: A required unique identifier for the source
              vertex (pointed from).
            </li>
            <li>
              <code>target</code>: A required unique identifier for the target
              vertex (pointed to).
            </li>
          </ul>
          <Paragraph>
            Note that edges need not have a direction and can be bidirectional.
            Additionally, multiple edges can exist between the same pair of
            vertices.
          </Paragraph>
        </Subsection>

        <GraphFormatDocs />

        <Subsection title="HyperThought&trade;">
          Carta currently supports external integration with the{" "}
          <a href="https://www.hyperthought.io/">HyperThought&trade;</a> API to
          obtain data from workflow processes. This requires the
          HyperThought&trade; API access key obtainable from your{" "}
          <a href="https://www.hyperthought.io/api/common/my_account/">
            account
          </a>
          .
        </Subsection>
      </Section>
    );
  }
}
