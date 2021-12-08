import { FunctionComponent } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for a property. */
const PropertyIcon: FunctionComponent<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <g transform="translate(0 -263.13)">
      <path
        d="m28.522 280.07a3.175 3.175 0 0 1-3.175 3.175 3.175 3.175 0 0 1-3.175-3.175 3.175 3.175 0 0 1 3.175-3.175 3.175 3.175 0 0 1 3.175 3.175zm0-6.35a3.175 3.175 0 0 1-3.175 3.175 3.175 3.175 0 0 1-3.175-3.175 3.175 3.175 0 0 1 3.175-3.175 3.175 3.175 0 0 1 3.175 3.175zm-8.4667 6.35a3.175 3.175 0 0 1-3.175 3.175 3.175 3.175 0 0 1-3.175-3.175 3.175 3.175 0 0 1 3.175-3.175 3.175 3.175 0 0 1 3.175 3.175zm0 6.35a3.175 3.175 0 0 1-3.175 3.175 3.175 3.175 0 0 1-3.175-3.175 3.175 3.175 0 0 1 3.175-3.175 3.175 3.175 0 0 1 3.175 3.175zm-8.4667 0a3.175 3.175 0 0 1-3.175 3.175 3.175 3.175 0 0 1-3.175-3.175 3.175 3.175 0 0 1 3.175-3.175 3.175 3.175 0 0 1 3.175 3.175zm0-12.7a3.175 3.175 0 0 1-3.175 3.175 3.175 3.175 0 0 1-3.175-3.175 3.175 3.175 0 0 1 3.175-3.175 3.175 3.175 0 0 1 3.175 3.175zm12.701-6.3495v2.8024a3.7042 3.7042 0 0 1 1.0578-0.15709 3.7042 3.7042 0 0 1 1.0573 0.15606v-2.8014zm2.1151 16.247a3.7042 3.7042 0 0 1-1.0573 0.1571 3.7042 3.7042 0 0 1-1.0578-0.15606v9.1529h2.1151zm-10.582-16.247v9.1524a3.7042 3.7042 0 0 1 1.0578-0.15709 3.7042 3.7042 0 0 1 1.0573 0.15606v-9.1514zm2.1151 22.597a3.7042 3.7042 0 0 1-1.0573 0.1571 3.7042 3.7042 0 0 1-1.0578-0.15606v2.8029h2.1151zm-10.582-22.597v2.8024a3.7042 3.7042 0 0 1 1.0578-0.15709 3.7042 3.7042 0 0 1 1.0594 0.15658v-2.8019zm2.1172 9.896a3.7042 3.7042 0 0 1-1.0594 0.15761 3.7042 3.7042 0 0 1-1.0578-0.15606v5.6048a3.7042 3.7042 0 0 1 1.0578-0.15709 3.7042 3.7042 0 0 1 1.0594 0.15658zm0 12.7a3.7042 3.7042 0 0 1-1.0594 0.15761 3.7042 3.7042 0 0 1-1.0578-0.15606v2.8029h2.1172zm-5.2931 0.68717a2.1169 2.1169 0 1 0 0 4.2324h25.4a2.1169 2.1169 0 1 0 0-4.2324zm0-25.4a2.1169 2.1169 0 1 0 0 4.2324h25.4a2.1169 2.1169 0 1 0 0-4.2324z"
        style={{
          color: "currentcolor",
          fill: "currentcolor",
        }}
      />
    </g>
  </svg>
);

export default PropertyIcon;