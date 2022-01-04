/** The various numeric axes for the data. */
declare type AxisName = "x" | "y" | "z";
/** The expected type of the plot passed into the plotting function. */
interface IPlot<TData> {
    /** The data to display in the plot itself. */
    data: TData;
    /** The size of the plot rectangle. */
    size?: {
        width: number;
        height: number;
    };
    /** The margins inside the plot rectangle between the edges and the plot. */
    margin?: {
        left?: number;
        right?: number;
        top?: number;
        bottom?: number;
    };
    /** The properties of the axes for the scatter plot. */
    axes?: Partial<Record<AxisName, {
        label?: string;
        minimum?: number;
        maximum?: number;
    }>>;
    /** The key of the data where the color information is located. */
    color?: string;
    /** The colormap to use for mapping values to colors. */
    colormap?: string;
}
/**
 * The data in the axis dimensions of the data are expected to be numbers. Any other specified field may be of any type.
 * For instance, colors may be strings.
 */
declare type IScatterData = (Record<AxisName, any> & Record<string, any>)[];
/**
 * Appends an SVG element containing a generated scatter plot to a container using specified plot data.
 * @param container The container that will have the SVG element plot appended to it.
 * @param plot The scatter plot information.
 */
declare const ScatterPlot: (container: HTMLElement, plot: IPlot<IScatterData>) => void;
export default ScatterPlot;
