import React, { Component } from "react";
import { Container } from "reactstrap";
import { GraphingDocs } from "./GraphingDocs";
import { DataFormatsDocs } from "./DataFormatsDocs";
import { ApiDocs } from "./ApiDocs";

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
