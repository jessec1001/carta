/**
 * A function used to update data in a plot.
 * @template TPlot The type of the plot data.
 */
type PlotterUpdate<TPlot> = (plot: TPlot) => void;
/**
 * A function used to plot data.
 * @template TPlot The type of the plot data.
 * @template TEvents The type of event listeners that the plot emits.
 */
type Plotter<TPlot, TEvents> = (
  container: HTMLElement,
  plot: TPlot,
  events?: TEvents
) => PlotterUpdate<TPlot>;

export type { Plotter, PlotterUpdate };
