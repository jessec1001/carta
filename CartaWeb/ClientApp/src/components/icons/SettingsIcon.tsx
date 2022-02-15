import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

const SettingsIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      transform="scale(.26459)"
      d="m52.535 10v15.691c-2.3132 0.69749-4.5375 1.6033-6.6562 2.6895l0.11524-0.11523-11.314-11.314-16.971 16.971 11.107 11.107c-1.127 2.0751-2.0669 4.2645-2.8164 6.5371v-0.10156h-16v24h15.691c0.69749 2.3132 1.6033 4.5375 2.6895 6.6562l-0.11523-0.11524-11.314 11.314 16.971 16.971 11.107-11.107c2.0751 1.127 4.2645 2.0669 6.5371 2.8164h-0.10156v16h24v-15.691c2.3132-0.69749 4.5375-1.6033 6.6562-2.6894l-0.11524 0.11523 11.314 11.314 16.971-16.971-11.107-11.107c1.1271-2.0753 2.0669-4.2643 2.8164-6.5371v0.10156h16v-24h-15.691c-0.69749-2.3132-1.6033-4.5375-2.6894-6.6562l0.11523 0.11524 11.314-11.314-16.971-16.969-11.107 11.105c-2.0753-1.1271-4.2643-2.0669-6.5371-2.8164h0.10156v-16h-24zm11.465 30.002c13.35 0 23.998 10.648 23.998 23.998 0 13.35-10.648 23.998-23.998 23.998s-23.998-10.648-23.998-23.998c0-13.35 10.648-23.998 23.998-23.998z"
      style={{
        fill: "currentcolor",
      }}
    />
  </svg>
);

export default SettingsIcon;
