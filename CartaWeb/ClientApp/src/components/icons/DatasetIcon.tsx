import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for a dataset (i.e. in a workspace). */
const DatasetIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      d="m16.934 8.4668-14.816 8.4668 14.816 8.4668 0.78711-0.45117 14.029-8.0156zm0 3.6563 8.418 4.8105-8.418 4.8105-8.418-4.8105z"
      style={{
        fill: "currentcolor",
      }}
    />
  </svg>
);

export default DatasetIcon;
