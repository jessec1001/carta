import { Plotter } from "types";
interface IGraphDatum {
    id: string;
    label: string | null;
    neighbors: string[];
}
interface IGraphPlot {
    data: IGraphDatum[];
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
interface IGraphInteraction {
}
declare const GraphPlot: Plotter<IGraphPlot, IGraphInteraction>;
export default GraphPlot;
