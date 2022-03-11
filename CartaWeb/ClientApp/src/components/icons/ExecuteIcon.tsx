import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for an execute symbol. */
const ExecuteIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      d="m6.3501 27.493v-21.119l19.43 10.56z"
      style={{
        fill: "none",
        stroke: "currentcolor",
        strokeWidth: 3.175,
      }}
    />
  </svg>
);

export default ExecuteIcon;
