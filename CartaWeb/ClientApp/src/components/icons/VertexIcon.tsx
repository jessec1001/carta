import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** The props used for the {@link VertexIcon} component. */
interface VertexIconProps {
  /** The color of the vertex that the icon represents. */
  color?: string;
  /** Whether the vertex that the icon represents is selected. */
  selected?: boolean;
}

/** An SVG icon that represents a vertex (i.e. in a graph). */
const VertexIcon: FC<IconProps & VertexIconProps> = ({
  color = "currentcolor",
  selected = false,
  children,
  ...props
}) => {
  if (selected) {
    return (
      <svg
        version="1.1"
        viewBox="0 0 33.867 33.867"
        xmlns="http://www.w3.org/2000/svg"
        style={SVGStyle(props)}
      >
        <path
          d="m16.934 2.1172c-8.158 0-14.816 6.6584-14.816 14.816 0 8.158 6.6584 14.816 14.816 14.816 8.158 0 14.816-6.6584 14.816-14.816 0-8.158-6.6584-14.816-14.816-14.816zm0 4.2324c5.8701 0 10.584 4.7138 10.584 10.584 0 5.8701-4.7138 10.584-10.584 10.584-5.8701 0-10.584-4.7138-10.584-10.584 0-5.8701 4.7138-10.584 10.584-10.584zm4.2333 10.584a4.2334 4.2334 0 0 1-4.2334 4.2334 4.2334 4.2334 0 0 1-4.2334-4.2334 4.2334 4.2334 0 0 1 4.2334-4.2334 4.2334 4.2334 0 0 1 4.2334 4.2334z"
          style={{
            fill: color,
          }}
        />
      </svg>
    );
  } else {
    return (
      <svg
        version="1.1"
        viewBox="0 0 33.867 33.867"
        xmlns="http://www.w3.org/2000/svg"
        style={SVGStyle(props)}
      >
        <path
          d="m16.934 2.1172c-8.158 0-14.816 6.6584-14.816 14.816 0 8.158 6.6584 14.816 14.816 14.816 8.158 0 14.816-6.6584 14.816-14.816 0-8.158-6.6584-14.816-14.816-14.816zm0 4.2324c5.8701 0 10.584 4.7138 10.584 10.584 0 5.8701-4.7138 10.584-10.584 10.584-5.8701 0-10.584-4.7138-10.584-10.584 0-5.8701 4.7138-10.584 10.584-10.584z"
          style={{
            fill: color,
          }}
        />
      </svg>
    );
  }
};

export default VertexIcon;
