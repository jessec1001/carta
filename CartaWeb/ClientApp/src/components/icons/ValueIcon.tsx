import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** The props for the {@link ValueIcon} component. */
interface ValueIconProps {
  /** The color of the property that this icon represents. */
  color?: string;
}

/** An SVG icon that represents a value (i.e. in data). */
const ValueIcon: FC<IconProps & ValueIconProps> = ({
  color = "currentcolor",
  children,
  ...props
}) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <g transform="matrix(2 0 0 2 -16.933 -543.21)">
      <circle
        cx="16.933"
        cy="280.07"
        r="3.175"
        style={{
          fill: color,
        }}
      />
    </g>
  </svg>
);

export default ValueIcon;
