import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for a refresh symbol. */
const RefreshIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      d="m16.495 4.9441 2.9934 2.9934-2.9934 2.9934m9.4343 6.0031a8.9965 8.9966 0 0 1-5.5536 8.3118 8.9965 8.9966 0 0 1-9.8044-1.9502 8.9965 8.9966 0 0 1-1.9502-9.8044 8.9965 8.9966 0 0 1 8.3117-5.5537"
      style={{
        fill: "none",
        stroke: "currentcolor",
        strokeLinecap: "square",
        strokeWidth: 3.175,
      }}
    />
  </svg>
);

export default RefreshIcon;
