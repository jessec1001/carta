import React, { Component } from "react";
import { Container } from "reactstrap";
import { GraphingDocs } from "./docs/GraphingDocs";
import { DataFormatsDocs } from "./docs/DataFormatsDocs";
import { ApiDocs } from "./docs/ApiDocs";

export default class DocsPage extends Component {
  static displayName = DocsPage.name;

  render() {
    return (
      <Container className="pb-4 mt-4">
        <h2>Documentation</h2>
        <GraphingDocs />
        <DataFormatsDocs />
        <ApiDocs />
      </Container>
    );
  }
}
