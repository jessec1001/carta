import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for an operation (i.e. in a workspace). */
const OperationIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      d="m7.7267 11.695-2.2454 2.2454 1.4056 1.4056h-5.2788v3.1761h5.2783l-1.4051 1.4051 2.2454 2.2448 5.2385-5.2385-2.2454-2.2448zm19.294 0-2.2448 2.2454 1.4056 1.4056h-3.6897v-3.1755h-9.5256v9.5256h9.5256v-3.174h3.6892l-1.4051 1.4051 2.2448 2.2448 5.2385-5.2385-2.2448-2.2448zm-10.881 3.651h3.1755v3.1761h-3.1755z"
      style={{
        fill: "currentcolor",
      }}
    />
  </svg>
);

export default OperationIcon;
