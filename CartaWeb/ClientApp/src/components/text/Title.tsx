import { FunctionComponent } from "react";
import Text, { TextProps } from "./Text";

/** The props used for the {@link Title} component. */
interface TitleProps {
  /**
   * The level of the title corresponding to the heading level.
   * For instance `1` corresponds with `<h1 />` and `4` corresponds with `<h4 />`.
   * If not specified, defaults to `1`. Any value `> 6` will use a `<div />` element.
   */
  level?: number;
}

/** A component that renders title text based on a specified heading level. Uses the {@link Text} component. */
const Title: FunctionComponent<TextProps & TitleProps> = ({
  component,
  level = 1,
  size = "title",
  children,
  ...props
}) => {
  // Use a switch statement to load the component corresponding to the heading level.
  if (!component) {
    switch (level) {
      case 1:
        component = ({ children, ...props }) => <h1 {...props}>{children}</h1>;
        break;
      case 2:
        component = ({ children, ...props }) => <h2 {...props}>{children}</h2>;
        break;
      case 3:
        component = ({ children, ...props }) => <h3 {...props}>{children}</h3>;
        break;
      case 4:
        component = ({ children, ...props }) => <h4 {...props}>{children}</h4>;
        break;
      case 5:
        component = ({ children, ...props }) => <h5 {...props}>{children}</h5>;
        break;
      case 6:
        component = ({ children, ...props }) => <h6 {...props}>{children}</h6>;
        break;
    }
  }

  // Render the base text component.
  return (
    <Text component={component} size={size} {...props}>
      {children}
    </Text>
  );
};

export default Title;
export type { TitleProps };
