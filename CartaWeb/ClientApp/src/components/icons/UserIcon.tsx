import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

const UserIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      transform="scale(.26459)"
      d="m64.002 8a28 28 0 0 0-28 28 28 28 0 0 0 13.643 24h-9.6465c-8.864 0-16 7.6451-16 17.143v42.855h80v-42.855c0-9.4971-7.136-17.143-16-17.143h-9.6328a28 28 0 0 0 13.637-24 28 28 0 0 0-28-28z"
      style={{
        strokeWidth: 3.7795,
        paintOrder: "normal",
        stroke: "currentcolor",
        fill: "currentcolor",
      }}
    />
  </svg>
);

export default UserIcon;
