import { Plotter } from "types";
interface IGraphVertex {
    id: string;
    label: string;
}
interface IGraphEdge {
    directed: boolean;
    source: string;
    target: string;
}
interface IGraphPlot {
    vertices: IGraphVertex[];
    edges: IGraphEdge[];
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
