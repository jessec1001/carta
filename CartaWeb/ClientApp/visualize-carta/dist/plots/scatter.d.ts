import { Plot, Plotter } from "types";
/** The type of datum for each scatter plot point. */
interface ScatterPlotDatum {
    /** The x-component of the datum. */
    x?: number;
    /** The y-component of the datum. */
    y?: number;
    /** The z-component of the datum. */
    z?: number;
    /** The radius of the datum. Defaults to 1.0 if not specified. */
    radius?: number;
    /** The value of the datum. Defaults to 0.0 if not specified. */
    value?: number;
    /** The optional styles to apply to the datum point. */
    style?: Partial<CSSStyleDeclaration>;
}
/** The type of the combined data for the scatter plot. */
interface ScatterPlot<TDatum extends ScatterPlotDatum = ScatterPlotDatum> extends Plot {
    type: "scatter";
    /** The data to display in the plot itself. */
    data: TDatum[];
    /** The colormap to use for mapping values to colors. */
    colormap?: string;
}
/**
 * Creates a scatter plot and attaches it to the specified container.
 * @param container The container to attach the plot to.
 * @param plot The data of the plot to generate.
 * @param events The events to attach to the plot.
 * @returns An updater function to update the plot.
 */
declare const PlotScatter: Plotter<ScatterPlot, {}>;
export default PlotScatter;
