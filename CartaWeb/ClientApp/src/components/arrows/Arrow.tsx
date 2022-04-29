import classNames from "classnames";
import { ComponentProps, CSSProperties, FC } from "react";
import { useArrows } from "./Context";
import styles from "./Arrow.module.css";

// TODO: Allow for the source and target props to instead be a position
/** The props for the {@link Arrow} component. */
interface ArrowProps
  extends Omit<ComponentProps<"svg">, "source" | "target" | "points"> {
  source: string | number | null;
  target: string | number | null;
  pathStyle?: CSSProperties;

  // points?: [x: number, y: number][];
  // curveRadius?: number;
  /** These values should be in the [0, 1] range so that [0, 0.5] is center left, etc. */
  // anchor?: [x: number, y: number];
}

const arrowSvgData = (
  source: [number, number],
  target: [number, number]
  // control: [number, number][],
  // radius: number
): string => {
  // TODO: Implement the curve radius parameter.
  //       This will require a generalization that tracks lines before and after control points.

  // Start the data at the source.
  let data = "";
  data += `M ${source[0]} ${source[1]} `;

  // // Add the control points.
  // for (const point of control) {
  //   data += `L ${point[0]} ${point[1]} `;
  // }

  // End the data at the target.
  data += `L ${target[0]} ${target[1]}`;
  return data;
};

/** A component that renders an SVG arrow from a source to a target with optional control points. */
const Arrow: FC<ArrowProps> = ({
  source,
  target,
  // points = [],
  // curveRadius = 0,
  pathStyle,
  className,
  ...props
}) => {
  // Get the arrows context.
  const { nodes } = useArrows();

  // Check if the source and target are valid.
  if (source === null || target === null) return null;

  // Get the source and target positions.
  const sourcePos = nodes(source).position();
  const targetPos = nodes(target).position();

  // Check if the source and target positions are valid.
  if (sourcePos === undefined || targetPos === undefined) return null;

  return (
    <svg
      width={"100%"}
      height={"100%"}
      className={classNames(styles.arrow, className)}
      {...props}
    >
      <path
        d={arrowSvgData(sourcePos, targetPos)}
        className={styles.clickable}
      />
      <path
        d={arrowSvgData(sourcePos, targetPos)}
        style={{
          fill: "none",
          stroke: "currentcolor",
          strokeWidth: "2px",
          ...pathStyle,
        }}
      />
    </svg>
  );
};

export default Arrow;
