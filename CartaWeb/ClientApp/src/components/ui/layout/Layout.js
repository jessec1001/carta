import React, { Component } from "react";
import { Container } from "reactstrap";
import { Navigation } from "components/ui/layout/navigation/Navigation";
import "./Layout.css";
import NotificationAlert from "components/ui/notifications/NotificationAlert";

export class Layout extends Component {
  static displayName = Layout.name;

  render() {
    return (
      <Container fluid className="h-100 d-flex flex-column">
        <Navigation />
        {this.props.children}
      </Container>
    );
  }
}
