import { PlotAxis } from "./PlotAxis";

/** Represents the base type for a generalized plot. */
interface Plot {
  /** The type of the plot. Used for automatically determining the plotter to use. */
  type?: string;

  /** The optional size to assign to the generated plot.  */
  size?: { width: number; height: number };
  /** The margins inside the plot rectangle between the edges and the plot. */
  margin?: {
    left?: number;
    right?: number;
    top?: number;
    bottom?: number;
  };
  /** The optional styles to apply to the generated plot. */
  style?: Partial<CSSStyleDeclaration>;

  // TODO: Implement.
  /** Information for all of the axes on a plot. */
  axes?: {
    x?: PlotAxis;
    y?: PlotAxis;
    z?: PlotAxis;
  };
}

export type { Plot };
