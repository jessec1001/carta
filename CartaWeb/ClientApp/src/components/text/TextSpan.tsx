import { FunctionComponent } from "react";
import Text, { TextProps } from "./Text";

/** A component that renders text in a span. */
const TextSpan: FunctionComponent<TextProps> = ({
  component = ({ children, ...props }) => <span {...props}>{children}</span>,
  children,
  ...props
}) => {
  return (
    <Text component={component} {...props}>
      {children}
    </Text>
  );
};

export default TextSpan;
