import { FunctionComponent } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for a dataset (i.e. in a workspace). */
const DatasetIcon: FunctionComponent<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      d="m2.1164 16.933 14.817-8.4667 14.817 8.4667-14.817 8.4668z"
      style={{
        fill: "none",
        strokeLinecap: "round",
        strokeLinejoin: "round",
        strokeWidth: 4.2334,
        stroke: "currentcolor",
      }}
    />
  </svg>
);

export default DatasetIcon;
