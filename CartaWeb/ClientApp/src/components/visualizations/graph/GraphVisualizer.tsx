import React, { Component } from "react";
import { Edge, Property } from "library/api/data";
import { GraphData } from "library/api/data";
import { DataNode } from "library/api/data/types";
import { Options } from "vis-network/standalone";
import { VisWrapper } from "components/wrappers";

export interface GraphVisualizerProps {
  graph: GraphData;
  onPropertiesChanged?: (properties: Property[]) => void;
}
export interface GraphVisualizerState {
  options: Options;
  cursor?: string;
  selection: string[];
}

export default class GraphVisualizer extends Component<
  GraphVisualizerProps,
  GraphVisualizerState
> {
  static displayName = GraphVisualizer.name;

  constructor(props: any) {
    super(props);

    // Set the default graph visualizer state.
    this.state = {
      options: this.getDefaultOptions(),
      selection: [],
    };

    // Bind event handlers.
    this.handleAddNode = this.handleAddNode.bind(this);
    this.handleAddEdge = this.handleAddEdge.bind(this);
    this.handleExecuteNode = this.handleExecuteNode.bind(this);
    this.handleClickNode = this.handleClickNode.bind(this);
    this.handleClickSpace = this.handleClickSpace.bind(this);
    this.handleHoverNode = this.handleHoverNode.bind(this);
    this.handleBlurNode = this.handleBlurNode.bind(this);
    this.handleDragStart = this.handleDragStart.bind(this);
    this.handleDragging = this.handleDragging.bind(this);
    this.handleDragEnd = this.handleDragEnd.bind(this);

    // Create and initialize the data view.
    const graph = this.props.graph;
    graph.visibleNodes.on("add", this.handleAddNode);
    graph.visibleEdges.on("add", this.handleAddEdge);
    graph.on("selectionChanged", () => {
      this.setState(() => {
        const selection = this.props.graph.selection;
        this.collectProperties(selection);
        return { selection };
      });
    });
    graph.on("dataChanged", () => {
      this.collectProperties(this.props.graph.selection);
    });
  }

  componentDidMount() {
    // Initialize the view data.
    this.props.graph
      .initialize()
      .then(() => {
        // Update options from the directedness of the graph.
        this.setState((state) => {
          const options = { ...state.options };
          const properties = this.props.graph.properties;
          if (properties) {
            if (properties.directed) (options as any).edges.arrows = "to";
          }
          return { options };
        });
      })
      .then(() => this.props.graph.colorGraph());
  }

  colorComponentsToHSL(components: {
    hue: number;
    saturation: number;
    lightness: number;
  }) {
    const h = `${360 * components.hue}`;
    const s = `${100 * components.saturation}%`;
    const l = `${100 * components.lightness}%`;
    return `hsl(${h},${s},${l})`;
  }
  colorComponentsToHSLA(
    components: {
      hue: number;
      saturation: number;
      lightness: number;
    },
    alpha: number
  ) {
    const h = `${360 * components.hue}`;
    const s = `${100 * components.saturation}%`;
    const l = `${100 * components.lightness}%`;
    return `hsla(${h},${s},${l},${alpha})`;
  }

  getDefaultOptions(): Options {
    return {
      nodes: {
        borderWidth: 3,
        borderWidthSelected: 3,
        size: 15,
        shape: "custom",
        ctxRenderer: this.getNodeRenderer(),
      },
      edges: {
        hoverWidth: 0,
        selectionWidth: 0,
      },
      interaction: {
        hover: true,
        hoverConnectedEdges: false,
        multiselect: true,
        selectConnectedEdges: false,
      },
    } as Options;
  }
  getNodeRenderer() {
    const that = this;
    return (args: {
      ctx: CanvasRenderingContext2D;
      id: string;
      x: number;
      y: number;
      state: { selected: boolean; hover: boolean };
      style: any;
      label?: string;
    }) => {
      const { ctx, x, y, style, state, label, id } = args;
      const { selected, hover } = state;

      const size: number = style.size;
      const borderSize: number = style.borderWidth;
      const fontSize: string = "12pt";

      const node = that.props.graph.visibleNodes.get(id) as DataNode;

      const colorStroke: string = style.color;
      const colorFill: string = node.expanded
        ? "white"
        : node.colorComponents
        ? that.colorComponentsToHSL({
            ...node.colorComponents,
            lightness: (1.0 + node.colorComponents.lightness) / 2.0,
          })
        : style.color;
      const colorSelectedNoHoverFill: string = node.colorComponents
        ? this.colorComponentsToHSL({
            ...node.colorComponents,
            lightness: 0.2 * node.colorComponents.lightness,
          })
        : style.color;
      const colorSelectedHoverFill: string = node.colorComponents
        ? this.colorComponentsToHSL({
            ...node.colorComponents,
            lightness: 0.4 * node.colorComponents.lightness,
          })
        : style.color;
      const colorHoverFill: string = node.colorComponents
        ? this.colorComponentsToHSL({
            ...node.colorComponents,
            lightness: 0.6 * node.colorComponents.lightness,
          })
        : style.color;

      return {
        drawNode: () => {
          // Draw out a circle.
          ctx.beginPath();
          ctx.arc(x, y, size, 0, 2 * Math.PI, false);
          ctx.closePath();

          // The circle should be filled in with the node color but partially transparent.
          ctx.fillStyle = colorFill;
          ctx.fill();

          // The circle should be stroked in with the node color.
          ctx.lineWidth = borderSize;
          ctx.strokeStyle = colorStroke;
          ctx.stroke();

          // If the node is loading, draw a spinning inner circle.
          if (typeof node.loading === "number") {
            const timeSeconds = (node.loading / 1000) % 2;
            const angle1 = timeSeconds * Math.PI;
            const angle2 = (timeSeconds + 1.75) * Math.PI;

            ctx.beginPath();
            ctx.arc(x, y, size / 2 + borderSize, angle1, angle2, false);
            ctx.strokeStyle = colorSelectedNoHoverFill;
            ctx.lineWidth = borderSize;
            ctx.stroke();
          }

          // If the node is selected or hovered, we draw a smaller circle inside to indicate it.
          if (selected || hover) {
            ctx.beginPath();
            ctx.arc(x, y, 2 * borderSize, 0, 2 * Math.PI, false);
            ctx.closePath();

            // The inner circle should be filled lighter when hovered and filled darker when selected.
            if (selected) {
              ctx.fillStyle = hover
                ? colorSelectedHoverFill
                : colorSelectedNoHoverFill;
            } else {
              ctx.fillStyle = colorHoverFill;
            }
            ctx.fill();
          }
        },
        drawExternalLabel: () => {
          // Draw the label for the node beneath the node itself.
          ctx.fillStyle = "black";
          ctx.font = `${fontSize} sans-serif`;
          ctx.textAlign = "center";
          ctx.textBaseline = "hanging";
          if (label) {
            ctx.fillText(label, x, y + 1.5 * size);
          }
        },
      };
    };
  }

  handleAddNode(eventType: "add", properties: { items: string[] }) {
    const graph = this.props.graph;
    const nodes = graph.nodes.get(properties.items);
    nodes.forEach((node) => {
      if (node.expanded) {
        graph.showNodeChildren(node.id);
      }
    });
  }
  handleAddEdge(eventType: "add", properties: { items: string[] }) {
    const graph = this.props.graph;
    const edges: Edge[] = graph.edges.get(properties.items);
    edges.forEach((edge) => {});
  }

  mergeProperties(properties: Property[]) {
    // Initialize our property mapping.
    const propertyMap: Record<string, Property> = {};

    // Compress the top-level properties for each of the node property arrays.
    properties.forEach((property) => {
      // Add property entry if not already added.
      if (!(property.id in propertyMap)) {
        propertyMap[property.id] = {
          id: property.id,
          values: [],
          properties: [],
        };
      }

      // Get stored property.
      const existingProperty = propertyMap[property.id];

      // Add values and subproperties to the property entry.
      existingProperty.values.push(...property.values);
      if (property.properties)
        existingProperty.properties!.push(...property.properties);
    });

    // For each property, compress its subproperties.
    Object.values(propertyMap).forEach((property) => {
      property.properties = this.mergeProperties(property.properties!);
    });
    return Object.values(propertyMap);
  }

  collectProperties(ids: string[]) {
    // Collect the properties for each node and flatten together.
    const graph = this.props.graph;
    const nodeProperties = graph.visibleNodes
      .get(ids)
      .map((node) => node.properties || [])
      .reduce((prev, next) => [...prev, ...next], []);
    const properties = this.mergeProperties(nodeProperties);

    if (this.props.onPropertiesChanged) {
      this.props.onPropertiesChanged(properties);
    }
  }

  handleExecuteNode(event: any) {
    // Swap the expanded or collapsed state of the double clicked node.
    const id = event.node as string;
    const graph = this.props.graph;
    const node: DataNode | null = graph.visibleNodes.get(id);
    if (node) {
      if (node.expanded) graph.collapseNode(id);
      else graph.expandNode(id);
    }

    // Remove the previous pair of selection states corresponding to the clicks.
    graph.workflow.undo(true);
    graph.workflow.undo(true);
  }
  handleClickNode(event: any) {
    const id = event.node as string;
    const graph = this.props.graph;
    if (graph.selection.includes(id)) graph.workflow.removeSelectorNodes([id]);
    else graph.workflow.addSelectorNodes([id]);
  }
  handleClickSpace(event: any) {
    const graph = this.props.graph;
    if (graph.workflow.getSelector().type !== "none")
      graph.workflow.applySelector({ type: "none" });
  }

  handleHoverNode() {
    this.setState({
      cursor: "pointer",
    });
  }
  handleBlurNode() {
    this.setState({
      cursor: "default",
    });
  }
  handleDragStart() {
    this.setState({
      cursor: "grabbing",
    });
  }
  handleDragging() {
    this.setState({
      cursor: "grabbing",
    });
  }
  handleDragEnd() {
    this.setState({
      cursor: "default",
    });
  }

  render() {
    return (
      <VisWrapper
        graph={this.props.graph}
        options={this.state.options}
        cursor={this.state.cursor}
        selection={this.props.graph.selection}
        onHoverNode={this.handleHoverNode}
        onBlurNode={this.handleBlurNode}
        onDragStart={this.handleDragStart}
        onDragging={this.handleDragging}
        onDragEnd={this.handleDragEnd}
        onExecuteNode={this.handleExecuteNode}
        onClickNode={this.handleClickNode}
        onClickSpace={this.handleClickSpace}
      />
    );
  }
}
