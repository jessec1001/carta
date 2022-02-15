import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for an arrow pointing upwards. */
const ArrowUpIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 16.934 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle({ ...props, half: true })}
  >
    <path
      d="m6.879 31.55h3.1756l-1.01e-4 -23.154 2.0361 2.0361 2.2458-2.2438-5.8696-5.8707-5.8696 5.8707 2.2459 2.2438 2.036-2.0361z"
      style={{
        fill: "currentcolor",
      }}
    />
  </svg>
);

export default ArrowUpIcon;
