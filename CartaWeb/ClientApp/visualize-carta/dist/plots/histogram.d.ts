declare type AxisName = "x" | "y";
interface IPlot<TData> {
    data: TData;
    size?: {
        width: number;
        height: number;
    };
    margin?: {
        left?: number;
        right?: number;
        top?: number;
        bottom?: number;
    };
    axes?: Partial<Record<AxisName, {
        label?: string;
        minimum?: number;
        maximum?: number;
    }>>;
    color?: string;
    colormap?: string;
}
declare type IHistogramData = {
    frequency: number;
    min: number;
    max: number;
}[];
declare const HistogramPlot: (container: HTMLElement, plot: IPlot<IHistogramData>) => void;
export default HistogramPlot;
