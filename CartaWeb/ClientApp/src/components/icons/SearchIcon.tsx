import { SVGStyle } from "./icons";

/** An SVG icon for a search (i.e. for a searchbar). */
const SearchIcon = () => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle}
  >
    <path
      d="m30.628 30.628-9.7598-9.7599m2.9449-7.1094a10.054 10.054 0 0 1-10.054 10.054 10.054 10.054 0 0 1-10.054-10.054 10.054 10.054 0 0 1 10.054-10.054 10.054 10.054 0 0 1 10.054 10.054z"
      style={{
        fill: "none",
        strokeLinecap: "square",
        strokeWidth: 4.2334,
        stroke: "currentColor",
      }}
    />
  </svg>
);

export default SearchIcon;
