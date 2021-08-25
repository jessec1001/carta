import React, { FunctionComponent } from "react";
import classNames from "classnames";

/** The props used for the {@link Text} component. */
interface TextProps {
  /**
   * A component that can be used to render the text.
   * This component should expect a class name and children.
   * If not specified, will default to a `<div />` component.
   */
  component?: React.ComponentType<any>;

  /** The size of the text to render. If not specified, defaults to the inherited style. */
  size?: "small" | "normal" | "medium" | "large" | "title";
  /** The color of the text to render. If not specified, defaults to the inherited style. */
  color?: "normal" | "muted" | "info" | "warning" | "error";
  /** The alignment of the text to render. If not specified, defaults to the inherited style. */
  align?: "left" | "right" | "center";
  /** The padding of the text to render. If not specified, defaults to the inerited style. */
  padding?: "top" | "bottom" | "center";
}

/**
 * A component that displays text with a specific size and color. Can be rendered semantically as any component but by
 * default renders as a basic block component.
 *
 * Note: this component was designed in name and convention to mimick the text component in React Native for forward
 * compatibility.
 */
const Text: FunctionComponent<TextProps> = ({
  component: element,
  size,
  color,
  align,
  padding,
  children,
  ...props
}) => {
  // We add the color and size values to the class name to use in CSS styling.
  // Notice that only the specified classes are added. Unspecified styles are not set allowing for them to be inherited.
  const className = classNames("text", {
    [`size-${size}`]: size,
    [`color-${color}`]: color,
    [`align-${align}`]: align,
    [`padded-${padding}`]: padding,
  });

  // We return the appropriate HTML element based on the semantics of element.
  return element ? (
    React.createElement(
      element,
      { className, ...props },
      ...React.Children.toArray(children)
    )
  ) : (
    <div className={className} {...props}>
      {children}
    </div>
  );
};

export default Text;
export type { TextProps };
