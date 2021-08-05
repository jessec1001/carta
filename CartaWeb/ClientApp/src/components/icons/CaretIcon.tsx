import { FunctionComponent } from "react";
import { IconProps, SVGStyle } from "./icons";

/** The props used for the {@link CaretIcon} component. */
interface CaretIconProps {
  /** The direction that the caret icon should face. */
  direction?: "down" | "up" | "left" | "right";
}

/** An SVG icon for a caret (i.e. for a dropdown). */
const CaretIcon: FunctionComponent<IconProps & CaretIconProps> = ({
  direction = "down",
  children,
  ...props
}) => {
  switch (direction) {
    case "down":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m5.3097 11.57 11.624 10.826 11.624-10.826"
            style={{
              fill: "none",
              strokeLinecap: "square",
              strokeWidth: 4.2334,
              stroke: "currentColor",
            }}
          />
        </svg>
      );
    case "up":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m28.558 22.297-11.624-10.826-11.624 10.826"
            style={{
              fill: "none",
              strokeLinecap: "square",
              strokeWidth: 4.2334,
              stroke: "currentColor",
            }}
          />
        </svg>
      );
    case "left":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m22.297 5.3095-10.826 11.624 10.826 11.624"
            style={{
              fill: "none",
              strokeLinecap: "square",
              strokeWidth: 4.2334,
              stroke: "currentColor",
            }}
          />
        </svg>
      );
    case "right":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m11.57 28.557 10.826-11.624-10.826-11.624"
            style={{
              fill: "none",
              strokeLinecap: "square",
              strokeWidth: 4.2334,
              stroke: "currentColor",
            }}
          />
        </svg>
      );
    default:
      return null;
  }
};

export default CaretIcon;
