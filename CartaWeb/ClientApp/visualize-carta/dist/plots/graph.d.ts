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
}
declare type IGraphData = {
    id: string;
    label: string | null;
    neighbors: string[];
}[];
declare const GraphPlot: (container: HTMLElement, plot: IPlot<IGraphData>) => void;
export default GraphPlot;
