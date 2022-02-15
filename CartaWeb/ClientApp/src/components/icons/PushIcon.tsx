import { FC } from "react";
import { IconProps, SVGStyle } from "./icons";

/** The props used for the {@link PushIcon} component. */
interface PushIconProps {
  /** The direction that the push icon should face. */
  direction?: "down" | "up" | "left" | "right";
}

/** An SVG icon for a push symbol (i.e. for a layout toolbar). */
const PushIcon: FC<IconProps & PushIconProps> = ({
  direction = "down",
  children,
  ...props
}) => {
  switch (direction) {
    case "down":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m29.434 2.3162h-25.4l-1e-7 3.174h11.111v19.98l-6.2694-6.2694-2.2459 2.2438 10.103 10.104 10.103-10.104-2.2459-2.2438-6.2694 6.2694v-19.98h11.113z"
            style={{
              strokeLinecap: "square",
              fill: "currentcolor",
            }}
          />
        </svg>
      );
    case "up":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m29.434 31.549h-25.4l-1e-7 -3.174h11.111v-19.98l-6.2694 6.2694-2.2459-2.2438 10.103-10.104 10.103 10.104-2.2459 2.2438-6.2694-6.2694v19.98h11.113z"
            style={{
              strokeLinecap: "square",
              fill: "currentcolor",
            }}
          />
        </svg>
      );
    case "left":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m31.35 4.2323v25.4h-3.174v-11.111h-19.98l6.2694 6.2694-2.2438 2.2459-10.104-10.103 10.104-10.103 2.2438 2.2459-6.2694 6.2694h19.98v-11.113z"
            style={{
              strokeLinecap: "square",
              fill: "currentcolor",
            }}
          />
        </svg>
      );
    case "right":
      return (
        <svg
          version="1.1"
          viewBox="0 0 33.867 33.867"
          xmlns="http://www.w3.org/2000/svg"
          style={SVGStyle(props)}
        >
          <path
            d="m2.1176 4.2323v25.4h3.174v-11.111h19.98l-6.2694 6.2694 2.2438 2.2459 10.104-10.103-10.104-10.103-2.2438 2.2459 6.2694 6.2694h-19.98v-11.113z"
            style={{
              strokeLinecap: "square",
              fill: "currentcolor",
            }}
          />
        </svg>
      );
  }
};

export default PushIcon;
