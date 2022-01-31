import { ComponentProps, FC } from "react";

// TODO: The source and target prop values should refer to identifiers of ArrowNodes.
/** The props for the {@link Arrow} component. */
interface ArrowProps
  extends Omit<ComponentProps<"svg">, "source" | "target" | "points"> {
  source: string | number | null;
  target: string | number | null;

  points?: [x: number, y: number][];
  curveRadius?: number;
}

const arrowSvgData = (
  source: [number, number],
  target: [number, number],
  control: [number, number][],
  radius: number
): string => {
  // TODO: Implement the curve radius parameter.
  //       This will require a generalization that tracks lines before and after control points.

  // Start the data at the source.
  let data = "";
  data += `M ${source[0]} ${source[1]} `;

  // Add the control points.
  for (const point of control) {
    data += `L ${point[0]} ${point[1]} `;
  }

  // End the data at the target.
  data += `L ${target[0]} ${target[1]}`;
  return data;
};

/** A component that renders an SVG arrow from a source to a target with optional control points. */
const Arrow: FC<ArrowProps> = ({
  source: s,
  target: t,
  points = [],
  curveRadius = 0,
  ...props
}) => {
  // TODO: In the future we will actually start at a source and end at a target with the points array representing
  //       the control points along the way.
  // This is the initial point of the arrow.
  const source = points.length > 0 ? points[0] : ([0, 0] as [number, number]);
  const target =
    points.length > 0
      ? points[points.length - 1]
      : ([0, 0] as [number, number]);
  const control = points.slice(1, points.length - 1);

  // We can calculate the width and height of the arrow from the bounds of the points.
  const minX = Math.min(source[0], target[0], ...points.map((d) => d[0]));
  const maxX = Math.max(source[0], target[0], ...points.map((d) => d[0]));
  const minY = Math.min(source[1], target[1], ...points.map((d) => d[1]));
  const maxY = Math.max(source[1], target[1], ...points.map((d) => d[1]));
  const width = maxX - minX;
  const height = maxY - minY;

  return (
    <svg width={width} height={height} {...props}>
      <path
        d={arrowSvgData(source, target, control, curveRadius)}
        fill="none"
        stroke="currentColor"
      />
    </svg>
  );
};

export default Arrow;
