import { Text, Section } from "components/text";
import { Component } from "react";

export class GraphingDocs extends Component {
  static displayName = GraphingDocs.name;

  render() {
    return (
      <Section title="Graphing">
        <Text>
          Carta provides the graphing view to explore and transform data. It has
          the following controls.
        </Text>
        <ul>
          <li>Select Vertex - left click (on vertex)</li>
          <li>
            Select Multiple Vertices - control key + left click (on vertex)
          </li>
          <li>Select Subtree - alt key + left click (on vertex)</li>
          <li>Move Vertex - left click + drag (on vertex)</li>
          <li>Pan - left click + drag</li>
          <li>Zoom - scroll wheel</li>
        </ul>

        <h4>Naming</h4>
        <Text>
          The properties contained in vertices can be changed in the{" "}
          <em>Naming</em> panel. Simply select the vertex or vertices with the
          properties that you would like to group together or rename. Then,
          click on the <em>Naming</em> button above the properties panel. By
          selecting a vertex in the left column and selecting vertices in the
          right column, you will rename the properties on the right to the
          property on the left.
        </Text>
      </Section>
    );
  }
}
