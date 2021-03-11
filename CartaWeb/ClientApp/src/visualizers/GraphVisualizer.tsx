import React, { Component } from "react";
import { Options } from "vis-network/standalone";
import {
  GraphDataView,
  registerGraph,
  createView,
  deleteView,
  initView,
  showViewChildrenVertices,
  hideViewChildrenVertices,
  updateViewVertex,
} from "../lib/graphstore";

import { Vis } from "../components/shared/graph/Vis";
import { Edge, Node, Property } from "../lib/types/graph";

export interface VisualizeNode extends Node {
  expanded?: boolean;
  colorSpace?: [number, number];
  colorComponents?: {
    hue: number;
    saturation: number;
    lightness: number;
  };
}

export interface GraphVisualizerProps {
  source: string;
  resource: string;
  parameters?: Record<string, any>;
  selector?: { type: string; [key: string]: any };

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

  view: GraphDataView;

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
    this.handleDoubleClick = this.handleDoubleClick.bind(this);
    this.handleSelectNode = this.handleSelectNode.bind(this);
    this.handleDeselectNode = this.handleDeselectNode.bind(this);
    this.handleHoverNode = this.handleHoverNode.bind(this);
    this.handleBlurNode = this.handleBlurNode.bind(this);
    this.handleDragStart = this.handleDragStart.bind(this);
    this.handleDragging = this.handleDragging.bind(this);
    this.handleDragEnd = this.handleDragEnd.bind(this);

    // Create and initialize the data view.
    const graphId = registerGraph(
      this.props.source,
      this.props.resource,
      this.props.parameters
    );
    const view = createView(graphId) as GraphDataView;
    view.nodes.on("add", this.handleAddNode);
    view.edges.on("add", this.handleAddEdge);
    this.view = view;
  }

  componentDidMount() {
    // Initialize the view data.
    initView(this.view)
      .then(() => {
        // Update options from the directedness of the graph.
        this.setState((state) => {
          const options = { ...state.options };
          if (this.view.properties.directed)
            (options as any).edges.arrows = "to";
          return { options };
        });
      })
      .then(() => this.colorGraph());
  }
  componentWillUnmount() {
    // Destroy view data.
    deleteView(this.view);
  }
  componentDidUpdate(prevProps: GraphVisualizerProps) {
    if (this.props.selector && this.props.selector !== prevProps.selector) {
      const selector = this.props.selector;
      let filter: (node: VisualizeNode) => boolean = (node: VisualizeNode) =>
        true;

      let pattern: string;
      switch (selector.type) {
        case "all":
          filter = () => true;
          break;
        case "none":
          filter = () => false;
          break;
        case "expanded":
          filter = (node) => !!node.expanded;
          break;
        case "collapsed":
          filter = (node) => !node.expanded;
          break;
        case "node":
          pattern = selector.pattern;
          if (
            pattern.length >= 2 &&
            pattern[0] === "/" &&
            pattern[pattern.length - 1] === "/"
          ) {
            pattern = pattern.substring(1, pattern.length - 1);
            filter = (node) =>
              !!node.label && !!node.label.match(new RegExp(pattern, "g"));
          } else {
            filter = (node) => {
              return !!node.label && node.label.includes(pattern);
            };
          }
          break;
        case "property":
          pattern = selector.pattern;
          if (
            pattern.length > 2 &&
            pattern[0] === "/" &&
            pattern[pattern.length - 1] === "/"
          ) {
            pattern = pattern.substring(1, pattern.length - 2);
            filter = (node) => {
              const regexp = new RegExp(pattern, "g");
              if (node.properties) {
                return node.properties.some(
                  (property) => property.id.match(regexp) !== null
                );
              }
              return false;
            };
          } else {
            filter = (node) => {
              if (node.properties) {
                return node.properties.some((property) =>
                  property.id.includes(pattern)
                );
              }
              return false;
            };
          }
          break;
        case "range":
          filter = (node) => {
            if (!node.properties) return false;
            const property = node.properties.find(
              (property) => property.id === selector.property
            );
            if (!property) return false;
            const observations = property.observations;
            return observations.some((observation) => {
              const value =
                typeof observation.value === "number"
                  ? observation.value
                  : parseFloat(observation.value);
              return value >= selector.min && value <= selector.max;
            });
          };
          break;
        case "descendants":
          const descendantIds: string[] = [];
          const openDescendantIds: string[] = [...this.state.selection];
          while (openDescendantIds.length > 0) {
            const descendantId = openDescendantIds.pop();
            const newDescendantIds = this.view.edges
              .get()
              .filter(edge => edge.from === descendantId)
              .map(edge => edge.to);
            
            descendantIds.push(...newDescendantIds);
            openDescendantIds.push(...newDescendantIds);
          }
          filter = (node) => descendantIds.includes(node.id);
          break;
        case "ancestors":
          const ancestorIds: string[] = [];
          const openAncestorIds: string[] = [...this.state.selection];
          while (openAncestorIds.length > 0) {
            const ancestorId = openAncestorIds.pop();
            const newAncestorIds = this.view.edges
              .get()
              .filter(edge => edge.to === ancestorId)
              .map(edge => edge.from);
            
              ancestorIds.push(...newAncestorIds);
            openAncestorIds.push(...newAncestorIds);
          }
          filter = (node) => ancestorIds.includes(node.id);
          break;
      }

      const selectedIds = this.view.nodes
        .get()
        .filter(filter)
        .map((node) => node.id);
      this.setState({
        selection: selectedIds,
      });
      this.collectProperties(selectedIds);
    }
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
  colorNode(node: VisualizeNode) {
    const parentIds = this.view.edges
      .get()
      .filter((edge) => edge.to === node.id)
      .map((edge) => edge.from);

    if (parentIds.length > 0) {
      // There is a parent node - for now we assume only one.
      const parentId = parentIds.shift() as string;
      const parent: VisualizeNode | null = this.view.nodes.get(parentId);
      if (parent) {
        if (parent.colorSpace) {
          // The parent is colored so we need to split the colorspace and assign this node a color.
          const childIds = this.view.edges
            .get()
            .filter((edge) => edge.from === parentId)
            .map((edge) => edge.to);

          // We split the parent colorspace evenly among the children. Then, the children get the specific color that
          // is the midway point of the colorspace.
          const parentColorSpace = parent.colorSpace;

          const splitSize =
            (parentColorSpace[1] - parentColorSpace[0]) /
            (childIds.length || 1);
          const splitIndex = childIds.indexOf(node.id);

          const colorSpace = [
            parentColorSpace[0] + splitSize * splitIndex,
            parentColorSpace[0] + splitSize * (splitIndex + 1),
          ];
          const colorComponents = {
            hue: (colorSpace[0] + colorSpace[1]) / 2,
            saturation: 1.0,
            lightness: 0.5,
          };

          updateViewVertex(this.view, {
            id: node.id,
            color: this.colorComponentsToHSL(colorComponents),
            colorSpace,
            colorComponents,
          });
        } else {
          // The parent is uncolored so we should color it first.
          this.colorNode(parent);
          this.colorNode(node);
        }
      }
    } else {
      // There is not a parent node so we color the node black.
      const colorSpace = [0.0, 1.0];
      const colorComponents = {
        hue: 0.0,
        saturation: 0.0,
        lightness: 0.0,
      };
      updateViewVertex(this.view, {
        id: node.id,
        color: this.colorComponentsToHSL(colorComponents),
        colorSpace,
        colorComponents,
      });
    }
  }
  colorGraph() {
    this.view.nodes
      .get()
      .filter((node: VisualizeNode) => !node.colorComponents)
      .forEach((node: VisualizeNode) => this.colorNode(node));
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
      label: string;
    }) => {
      const { ctx, x, y, style, state, label, id } = args;
      const { selected, hover } = state;

      const size: number = style.size;
      const borderSize: number = style.borderWidth;
      const fontSize: string = "12pt";

      const node: VisualizeNode = that.view.nodes.get(id) as VisualizeNode;

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
            lightness: 0.1 * node.colorComponents.lightness,
          })
        : style.color;
      const colorSelectedHoverFill: string = node.colorComponents
        ? this.colorComponentsToHSL({
            ...node.colorComponents,
            lightness: 0.2 * node.colorComponents.lightness,
          })
        : style.color;
      const colorHoverFill: string = node.colorComponents
        ? this.colorComponentsToHSL({
            ...node.colorComponents,
            lightness: 0.3 * node.colorComponents.lightness,
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
          ctx.fillText(label, x, y + 1.5 * size);
        },
      };
    };
  }

  handleAddNode(eventType: "add", properties: { items: string[] }) {
    const nodes: VisualizeNode[] = this.view.nodes.get(properties.items);
    nodes.forEach((node) => {
      if (node.expanded) {
        showViewChildrenVertices(this.view as GraphDataView, node.id);
      }
    });
  }
  handleAddEdge(eventType: "add", properties: { items: string[] }) {
    const edges: Edge[] = this.view.edges.get(properties.items);
    edges.forEach((edge) => {});
  }

  collectProperties(ids: string[]) {
    const properties: Property[] = [];
    this.view.nodes.get(ids).forEach((node: VisualizeNode) => {
      if (node.properties) {
        node.properties.forEach((nodeProperty) => {
          let property = properties.find(
            (property) => nodeProperty.id === property.id
          );
          if (!property) {
            property = {
              id: nodeProperty.id,
              observations: [],
            };
            properties.push(property);
          }
          property.observations.push(...nodeProperty.observations);
        });
      }
    });

    if (this.props.onPropertiesChanged) {
      this.props.onPropertiesChanged(properties);
    }
  }

  handleDoubleClick(event: any) {
    const ids: string[] = event.nodes;
    ids.forEach((id) => {
      // Swap the expanded or collapsed state of the double clicked nodes.
      const node: VisualizeNode | null = this.view.nodes.get(id);
      if (node) {
        updateViewVertex(this.view, {
          id: node.id,
          expanded: !node.expanded,
        });
        if (node.expanded)
          hideViewChildrenVertices(this.view, id).then(() => this.colorGraph());
        else
          showViewChildrenVertices(this.view, id).then(() => this.colorGraph());
      }
    });
  }
  handleSelectNode(event: any) {
    const nodeIds: string[] = event.nodes;
    this.collectProperties(nodeIds);
    this.setState({
      selection: nodeIds
    });
  }
  handleDeselectNode(event: any) {
    const nodeIds: string[] = event.nodes;
    this.collectProperties(nodeIds);
    this.setState({
      selection: nodeIds
    });
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
      cursor: "pointer",
    });
  }

  render() {
    return (
      <Vis
        graph={this.view as any}
        options={this.state.options}
        cursor={this.state.cursor}
        selection={this.state.selection}
        onDoubleClick={this.handleDoubleClick}
        onHoverNode={this.handleHoverNode}
        onBlurNode={this.handleBlurNode}
        onDragStart={this.handleDragStart}
        onDragging={this.handleDragging}
        onDragEnd={this.handleDragEnd}
        onSelectNode={this.handleSelectNode}
        onDeselectNode={this.handleDeselectNode}
      />
    );
  }
}
