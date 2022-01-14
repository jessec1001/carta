declare type PlotterUpdate<TPlot> = (plot: TPlot) => void;
declare type Plotter<TPlot, TInteraction> = (container: HTMLElement, plot: TPlot, interaction?: TInteraction) => PlotterUpdate<TPlot>;
export type { Plotter, PlotterUpdate };
