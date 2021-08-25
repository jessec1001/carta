import { FunctionComponent } from "react";
import Text, { TextProps } from "./Text";

/** A component that renders paragraph text. */
const Paragraph: FunctionComponent<TextProps> = ({
  component = ({ children, ...props }) => <p {...props}>{children}</p>,
  children,
  ...props
}) => {
  return (
    <Text component={component} {...props}>
      {children}
    </Text>
  );
};

export default Paragraph;
