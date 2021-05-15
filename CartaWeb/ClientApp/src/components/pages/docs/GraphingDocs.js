import React, { Component } from "react";

export class GraphingDocs extends Component {
    static displayName = GraphingDocs.name;
    
    render() {
        return (
            <section>
                <h3>Graphing</h3>
                <p>
                    Carta provides the graphing view to explore and transform data. It has the following controls.
                </p>
                <ul>
                    <li>Select Node - left click (on node)</li>
                    <li>Select Multiple Nodes - control key + left click (on node)</li>
                    <li>Select Subtree - alt key + left click (on node)</li>
                    <li>Move Node - left click + drag (on node)</li>
                    <li>Pan - left click + drag</li>
                    <li>Zoom - scroll wheel</li>
                </ul>

                <h4>Naming</h4>
                <p>
                    The properties contained in nodes can be changed in the <em>Naming</em> panel. Simply select the node or nodes with the properties that you would like to group together or rename. Then, click on the <em>Naming</em> button above the properties panel. By selecting a node in the left column and selecting nodes in the right column, you will rename the properties on the right to the property on the left.
                </p>
            </section>
        );
    }
}