import { FunctionComponent } from "react";
import { IconProps, SVGStyle } from "./icons";

import "./icons.css";

/** The props used for the {@link LoadingIcon} component. */
interface LoadingIconProps {
  /** Whether the loading icon should be animated or not. */
  animated?: boolean;
}

/** An SVG icon for loading a resource. Can be optionally animated. */
const LoadingIcon: FunctionComponent<IconProps & LoadingIconProps> = ({
  animated = true,
  children,
  ...props
}) => (
  <span
    className={animated ? "spinning" : undefined}
    style={{
      ...SVGStyle(props),
      display: "inline-block",
      width: "1em",
      height: "1em",
    }}
  >
    <svg
      version="1.1"
      viewBox="0 0 33.867 33.867"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="m30.163 16.934a13.23 13.23 0 0 1-8.167 12.223 13.23 13.23 0 0 1-14.418-2.8679 13.23 13.23 0 0 1-2.8679-14.418 13.23 13.23 0 0 1 12.223-8.167"
        style={{
          fill: "none",
          stroke: "currentcolor",
          strokeLinecap: "square",
          strokeWidth: 3.175,
        }}
      />
    </svg>
  </span>
);

export default LoadingIcon;
