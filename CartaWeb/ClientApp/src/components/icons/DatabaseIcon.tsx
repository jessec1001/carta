import { FunctionComponent } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for a database (i.e. in a workspace). */
const DatabaseIcon: FunctionComponent<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      transform="scale(.26459)"
      d="m64 8.002-55.998 32 21 12-21 11.998 21 12-21.002 12 56 32 56-32-21-12 20.998-12-20.998-12 20.998-11.998-55.998-32zm0 13.818 31.816 18.182-8.9043 5.0879-0.005859-0.001953-22.906 13.088-22.906-13.088-0.003906 0.001953-8.9043-5.0879 31.814-18.182zm22.908 37.088 8.9082 5.0918-8.9082 5.0898h-0.001953l-22.906 13.09-22.906-13.09-8.9082-5.0898 8.9023-5.0898 22.912 13.092 15.418-8.8145 7.4902-4.2793zm-45.814 24.002 22.906 13.09 22.906-13.09 8.9082 5.0898-31.814 18.18-31.814-18.18 8.9082-5.0898z"
      style={{
        fill: "currentcolor",
      }}
    />
  </svg>
);

export default DatabaseIcon;
