import { SVGStyle } from "./icons";

/** An SVG icon for a database (i.e. in a workspace). */
const DatabaseIcon = () => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle}
  >
    <path
      transform="scale(.26459)"
      d="m28.998 76-21 12 56.002 32 56.002-32-21.002-12-35 20zm-20.999-36.002 56-31.999 56 31.999-56 32zm20.999 12.002-21 12 56.002 32 56-32-21-12-35 20z"
      style={{
        fill: "none",
        strokeLinecap: "round",
        strokeLinejoin: "round",
        strokeWidth: 16,
        stroke: "currentcolor",
      }}
    />
  </svg>
);

export default DatabaseIcon;
