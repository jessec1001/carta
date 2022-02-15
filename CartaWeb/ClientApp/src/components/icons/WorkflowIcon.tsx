import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** An SVG icon for a workflow (i.e. in a workspace). */
const WorkflowIcon: FC<IconProps> = ({ children, ...props }) => (
  <svg
    version="1.1"
    viewBox="0 0 33.867 33.867"
    xmlns="http://www.w3.org/2000/svg"
    style={SVGStyle(props)}
  >
    <path
      transform="scale(.26459)"
      d="m64.688 6.2012-8.4902 8.4883 5.3125 5.3086h-17.506v-11.996h-36.002v6.002 30h17.996 18.006v-12.004h17.508l-5.3145 5.3145 8.4902 8.4824 19.311-19.312v17.52h18.004 17.996v-36.002h-36v6.002 11.508l-19.311-19.311zm37.314 37.803-19.799 19.797 8.4824 8.4824 5.3145-5.3105v29.516h-51.996v-12.49h-12.004v-17.025l5.3145 5.3105 8.4824-8.4824-19.799-19.797-19.797 19.797 8.4883 8.4824 5.3086-5.3086v17.023h-11.996v6.002 29.998h36.002v-11.516h64v-11.994-29.512l5.3066 5.3066 8.4902-8.4824-19.799-19.797zm-82.004-24.006h12.002v12.002h-12.002v-12.002zm76.002 0h12.004v12.002h-12.004v-12.002zm-76.002 76.49h12.002v11.516h-12.002v-11.516z"
      style={{
        strokeLinecap: "square",
        fill: "currentcolor",
      }}
    />
  </svg>
);

export default WorkflowIcon;
