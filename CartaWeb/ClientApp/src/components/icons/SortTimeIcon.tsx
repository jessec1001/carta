import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An icon to indicate a temporal sort. */
const SortTimeIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <g
      style={{
        fill: "none",
        stroke: "currentColor",
      }}
    >
      <ellipse
        cx="16.934"
        cy="16.934"
        rx="11.113"
        ry="11.113"
        style={{
          strokeWidth: 3.175,
        }}
      />
      <path
        d="m16.934 10.583v6.3501l-3.7042 3.175"
        style={{
          strokeLinecap: "round",
          strokeLinejoin: "round",
          strokeWidth: 2.1167,
        }}
      />
      <g
        style={{
          strokeLinejoin: "round",
          strokeWidth: 1.0583,
        }}
      >
        <path d="m28.046 16.934h-2.6459" />
        <path d="m26.557 22.49-2.2914-1.3229" />
        <path d="m22.49 26.557-1.3229-2.2914" />
        <path d="m16.934 28.046-1e-6 -2.6459" />
        <path d="m11.377 26.557 1.3229-2.2914" />
        <path d="m7.3097 22.49 2.2914-1.3229" />
        <path d="m5.8209 16.934 2.6459-1e-6" />
        <path d="m7.3097 11.377 2.2914 1.3229" />
        <path d="m11.377 7.3097 1.3229 2.2914" />
        <path d="m16.934 5.8209 1e-6 2.6459" />
        <path d="m22.49 7.3097-1.3229 2.2914" />
        <path d="m26.557 11.377-2.2914 1.3229" />
      </g>
    </g>
  </svg>
);

export default SortTimeIcon;
