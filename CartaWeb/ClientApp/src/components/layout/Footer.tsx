import { FunctionComponent } from "react";
import { Column, Row } from "components/structure";
import { version } from "../../../package.json";

/** A component that displays a footer at the bottom of a page. */
const Footer: FunctionComponent = ({ children, ...props }) => {
  return (
    <footer className="footer" {...props}>
      <Row>
        <Column>
          <span style={{ textAlign: "left" }}>
            &copy; 2021 Contextualize, LLC
          </span>
        </Column>
        <Column>
          <span style={{ textAlign: "right" }}>Carta v{version}</span>
        </Column>
      </Row>
    </footer>
  );
};

export default Footer;
