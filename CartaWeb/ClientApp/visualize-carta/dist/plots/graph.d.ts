import { Plotter } from "types";
interface IGraphVertex {
    id: string;
    label?: string;
    selected?: boolean;
    depth?: number;
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
    onClickNode?: (node: IGraphVertex) => void;
    onClickSpace?: () => void;
}
declare const GraphPlot: Plotter<IGraphPlot, IGraphInteraction>;
export default GraphPlot;
export type { IGraphPlot, IGraphVertex, IGraphEdge, IGraphInteraction };
