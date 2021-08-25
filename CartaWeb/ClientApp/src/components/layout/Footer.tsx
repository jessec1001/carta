import { FunctionComponent } from "react";
import { Column, Row } from "components/structure";
import { Text } from "components/text";
import { version } from "../../../package.json";

/** A component that displays a footer at the bottom of a page. */
const Footer: FunctionComponent = ({ children, ...props }) => {
  return (
    <footer className="footer" {...props}>
      <Row>
        <Column>
          <Text color="muted" align="left">
            &copy; 2021 Contextualize, LLC
          </Text>
        </Column>
        <Column>
          <Text color="muted" align="right">
            Carta v{version}
          </Text>
        </Column>
      </Row>
    </footer>
  );
};

export default Footer;
