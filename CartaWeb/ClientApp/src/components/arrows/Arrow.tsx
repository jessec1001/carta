import { ComponentProps, FC } from "react";
import { useArrows } from "./Context";

// TODO: Allow for the source and target props to instead be a position
/** The props for the {@link Arrow} component. */
interface ArrowProps
  extends Omit<ComponentProps<"svg">, "source" | "target" | "points"> {
  source: string | number | null;
  target: string | number | null;

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

  // TODO: In the future we will actually start at a source and end at a target with the points array representing
  //       the control points along the way.

  // We can calculate the width and height of the arrow from the bounds of the points.
  const minX = Math.min(sourcePos[0], targetPos[0]);
  const maxX = Math.max(sourcePos[0], targetPos[0]);
  const minY = Math.min(sourcePos[1], targetPos[1]);
  const maxY = Math.max(sourcePos[1], targetPos[1]);
  const width = maxX - minX;
  const height = maxY - minY;

  return (
    <svg width={width} height={height} {...props}>
      <path
        d={arrowSvgData(sourcePos, targetPos)}
        fill="none"
        stroke="currentColor"
      />
    </svg>
  );
};

export default Arrow;
