import React, { Component, HTMLProps } from "react";
import {
  Navbar,
  Nav,
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
} from "reactstrap";
import { GraphData, GraphWorkflow } from "../../../lib/graph";
import Action from "../../../lib/types/actions";
import Selector from "../../../lib/types/selectors";

import "./GraphToolbar.css";

interface GraphToolbarProps extends HTMLProps<HTMLDivElement> {
  graph?: GraphData;

  onOpenGraph?: () => void;
  onCloseGraph?: () => void;
  onDuplicateGraph?: () => void;

  onCreateWorkflow?: () => Promise<any>;
  onApplyWorkflow?: () => void;
}

export class GraphToolbar extends Component<GraphToolbarProps> {
  constructor(props: GraphToolbarProps) {
    super(props);

    this.handleSelector = this.handleSelector.bind(this);
    this.handleAction = this.handleAction.bind(this);

    this.handleSelectAll = this.handleSelectAll.bind(this);
    this.handleSelectNone = this.handleSelectNone.bind(this);
    this.handleSelectExpanded = this.handleSelectExpanded.bind(this);
    this.handleSelectCollapsed = this.handleSelectCollapsed.bind(this);
    this.handleSelectVertexName = this.handleSelectVertexName.bind(this);
    this.handleSelectPropertyName = this.handleSelectPropertyName.bind(this);
    this.handleSelectPropertyRange = this.handleSelectPropertyRange.bind(this);
    this.handleSelectDescendants = this.handleSelectDescendants.bind(this);
    this.handleSelectAncestors = this.handleSelectAncestors.bind(this);

    this.handleActionToNumber = this.handleActionToNumber.bind(this);
    this.handleActionIncrement = this.handleActionIncrement.bind(this);
    this.handleActionDecrement = this.handleActionDecrement.bind(this);
    this.handleActionStringReplace = this.handleActionStringReplace.bind(this);
    this.handleActionMean = this.handleActionMean.bind(this);
    this.handleActionMedian = this.handleActionMedian.bind(this);
    this.handleActionDeviation = this.handleActionDeviation.bind(this);
    this.handleActionVariance = this.handleActionVariance.bind(this);
    this.handleActionAggregate = this.handleActionAggregate.bind(this);
    this.handleActionPropagate = this.handleActionPropagate.bind(this);

    this.handleExpand = this.handleExpand.bind(this);
    this.handleCollapse = this.handleCollapse.bind(this);

    this.handleOpenGraph = this.handleOpenGraph.bind(this);
    this.handleCloseGraph = this.handleCloseGraph.bind(this);
    this.handleDuplicateGraph = this.handleDuplicateGraph.bind(this);

    this.handleCreateWorkflow = this.handleCreateWorkflow.bind(this);
    this.handleApplyWorkflow = this.handleApplyWorkflow.bind(this);
    this.handleClearWorkflow = this.handleClearWorkflow.bind(this);

    this.handleUndo = this.handleUndo.bind(this);
    this.handleRedo = this.handleRedo.bind(this);
    this.handleKeyboard = this.handleKeyboard.bind(this);
  }

  handleSelector(selector: Selector) {
    const graph = this.props.graph;
    if (graph) {
      graph.workflow.applySelector(selector);
    }
  }
  handleAction(action: Action) {
    const graph = this.props.graph;
    if (graph) {
      graph.workflow.applyAction(action);
    }
  }

  handleSelectAll() {
    this.handleSelector({
      type: "all",
    });
  }
  handleSelectNone() {
    this.handleSelector({
      type: "none",
    });
  }
  handleSelectExpanded() {
    this.handleSelector({
      type: "expanded",
    });
  }
  handleSelectCollapsed() {
    this.handleSelector({
      type: "collapsed",
    });
  }
  handleSelectVertexName() {
    let pattern = prompt("Enter a regular expression that matches node names.");
    if (pattern) {
      this.handleSelector({
        type: "vertex name",
        pattern: pattern,
      });
    }
  }
  handleSelectPropertyName() {
    let pattern = prompt(
      "Enter a regular expression that matches property names."
    );
    if (pattern) {
      this.handleSelector({
        type: "property name",
        pattern: pattern,
      });
    }
  }
  handleSelectPropertyRange() {
    let property = prompt("Enter the property name.");
    let rangeMin = parseFloat(
      prompt("Enter the lower bound of the range.") ?? ""
    );
    let rangeMax = parseFloat(
      prompt("Enter the upper bound of the range.") ?? ""
    );
    if (property) {
      this.handleSelector({
        type: "property range",
        property: property,
        minimum: rangeMin,
        maximum: rangeMax,
      });
    }
  }
  handleSelectDescendants() {
    this.handleSelector({
      type: "vertex descendants",
    });
  }
  handleSelectAncestors() {
    this.handleSelector({
      type: "vertex ancestors",
    });
  }

  handleActionToNumber() {
    this.handleAction({
      type: "to number",
    });
  }
  handleActionIncrement() {
    let amount = parseFloat(prompt("Enter the amount to increment by.") ?? "");
    this.handleAction({
      type: "increment",
      amount,
    });
  }
  handleActionDecrement() {
    let amount = parseFloat(prompt("Enter the amount to decrement by.") ?? "");
    this.handleAction({
      type: "decrement",
      amount,
    });
  }
  handleActionStringReplace() {
    let pattern =
      prompt("Enter a regular expression pattern to match string values.") ??
      "";
    let replacement = prompt("Enter the replacement string.") ?? "";
    this.handleAction({
      type: "string replace",
      pattern,
      replacement,
    });
  }
  handleActionMean() {
    this.handleAction({
      type: "mean",
    });
  }
  handleActionMedian() {
    this.handleAction({
      type: "median",
    });
  }
  handleActionDeviation() {
    this.handleAction({
      type: "deviation",
    });
  }
  handleActionVariance() {
    this.handleAction({
      type: "variance",
    });
  }
  handleActionAggregate() {
    this.handleAction({
      type: "aggregate",
    });
  }
  handleActionPropagate() {
    this.handleAction({
      type: "propagate",
    });
  }

  handleExpand() {
    const graph = this.props.graph;
    if (graph) {
      graph.selection.forEach((id) => graph.expandNode(id));
    }
  }
  handleCollapse() {
    const graph = this.props.graph;
    if (graph) {
      graph.selection.forEach((id) => graph.collapseNode(id));
    }
  }

  handleUndo() {
    const graph = this.props.graph;
    if (graph) {
      graph.workflow.undo();
    }
  }
  handleRedo() {
    const graph = this.props.graph;
    if (graph) {
      graph.workflow.redo();
    }
  }

  handleOpenGraph() {
    if (this.props.onOpenGraph) {
      this.props.onOpenGraph();
    }
  }
  handleCloseGraph() {
    if (this.props.onCloseGraph) {
      this.props.onCloseGraph();
    }
  }
  handleDuplicateGraph() {
    if (this.props.onDuplicateGraph) {
      this.props.onDuplicateGraph();
    }
  }

  handleCreateWorkflow() {
    const graph = this.props.graph;
    if (graph) {
      const workflow = new GraphWorkflow(
        undefined,
        this.props.onCreateWorkflow
      );
      workflow._create();
      graph.setWorkflow(workflow);
    }
  }
  handleApplyWorkflow() {
    if (this.props.onApplyWorkflow) {
      this.props.onApplyWorkflow();
    }
  }
  handleClearWorkflow() {
    const graph = this.props.graph;
    if (graph) {
      const workflow = new GraphWorkflow(
        undefined,
        this.props.onCreateWorkflow
      );
      graph.setWorkflow(workflow);
    }
  }

  handleKeyboard(event: KeyboardEvent) {
    if (event.ctrlKey && event.key.toLowerCase() === "o") {
      this.handleOpenGraph();
      event.preventDefault();
    }
    if (event.ctrlKey && event.key.toLowerCase() === "l") {
      this.handleCloseGraph();
      event.preventDefault();
    }

    if (event.ctrlKey && event.key.toLowerCase() === "z") {
      this.handleUndo();
      event.preventDefault();
    }
    if (event.ctrlKey && event.key.toLowerCase() === "y") {
      this.handleRedo();
      event.preventDefault();
    }

    if (event.ctrlKey && event.shiftKey && event.key.toLowerCase() === "a") {
      this.handleSelectNone();
      event.preventDefault();
    } else if (event.ctrlKey && event.key.toLowerCase() === "a") {
      this.handleSelectAll();
      event.preventDefault();
    }

    if (event.ctrlKey && event.key === "[") {
      this.handleSelectCollapsed();
    } else if (event.key === "[") {
      this.handleExpand();
    }
    if (event.ctrlKey && event.key === "]") {
      this.handleSelectExpanded();
    } else if (event.key === "]") {
      this.handleCollapse();
    }
  }

  componentDidMount() {
    document.addEventListener("keydown", this.handleKeyboard);
  }
  componentWillUnmount() {
    document.removeEventListener("keydown", this.handleKeyboard);
  }

  render() {
    const {
      graph,
      onCreateWorkflow,
      onApplyWorkflow,
      onOpenGraph,
      onCloseGraph,
      onDuplicateGraph,
      ...rest
    } = this.props;
    return (
      <Navbar light expand="md" {...rest}>
        <Nav navbar>
          <UncontrolledDropdown nav inNavbar>
            <DropdownToggle nav>Graph</DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={this.handleOpenGraph}>
                Open Graph<span className="shortcut">Ctrl+O</span>
              </DropdownItem>
              <DropdownItem onClick={this.handleCloseGraph}>
                Close Graph<span className="shortcut">Ctrl+L</span>
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleDuplicateGraph}>
                Duplicate Graph
              </DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown>
          <UncontrolledDropdown>
            <DropdownToggle nav>Workflow</DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={this.handleCreateWorkflow}>
                Create Workflow
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleApplyWorkflow}>
                Apply Workflow
              </DropdownItem>
              <DropdownItem onClick={this.handleClearWorkflow}>
                Clear Workflow
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleUndo}>
                Undo <span className="shortcut">Ctrl+Z</span>
              </DropdownItem>
              <DropdownItem onClick={this.handleRedo}>
                Redo <span className="shortcut">Ctrl+Y</span>
              </DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown>
          <UncontrolledDropdown nav inNavbar>
            <DropdownToggle nav>Selection</DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={this.handleSelectAll}>
                Select All <span className="shortcut">Ctrl+A</span>
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectNone}>
                Select None <span className="shortcut">Ctrl+Shift+A</span>
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleSelectExpanded}>
                Select Expanded <span className="shortcut">Ctrl+[</span>
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectCollapsed}>
                Select Collapsed <span className="shortcut">Ctrl+]</span>
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleSelectVertexName}>
                Select Node Name
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectPropertyName}>
                Select Property Name
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectPropertyRange}>
                Select Range
              </DropdownItem>
              {/* <hr className="divider" />
              <DropdownItem onClick={this.handleSelectDescendants}>
                Select Descendants
              </DropdownItem>
              <DropdownItem onClick={this.handleSelectAncestors}>
                Select Ancestors
              </DropdownItem> */}
            </DropdownMenu>
          </UncontrolledDropdown>
          <UncontrolledDropdown nav inNavbar>
            <DropdownToggle nav>Action</DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={this.handleActionToNumber}>
                Convert to Number
              </DropdownItem>
              <DropdownItem onClick={this.handleActionIncrement}>
                Increment Number
              </DropdownItem>
              <DropdownItem onClick={this.handleActionDecrement}>
                Decrement Number
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleActionStringReplace}>
                Replace String
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleActionMean}>
                Sample Mean
              </DropdownItem>
              <DropdownItem onClick={this.handleActionMedian}>
                Sample Median
              </DropdownItem>
              <DropdownItem onClick={this.handleActionDeviation}>
                Sample Standard Deviation
              </DropdownItem>
              <DropdownItem onClick={this.handleActionVariance}>
                Sample Variance
              </DropdownItem>
              <hr className="divider" />
              <DropdownItem onClick={this.handleActionAggregate}>
                Aggregate Observations
              </DropdownItem>
              <DropdownItem onClick={this.handleActionPropagate}>
                Propagate Observations
              </DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown>
          <UncontrolledDropdown nav inNavbar>
            <DropdownToggle nav>Navigation</DropdownToggle>
            <DropdownMenu>
              <DropdownItem onClick={this.handleExpand}>
                Expand Nodes <span className="shortcut">[</span>
              </DropdownItem>
              <DropdownItem onClick={this.handleCollapse}>
                Collapse Nodes <span className="shortcut">]</span>
              </DropdownItem>
            </DropdownMenu>
          </UncontrolledDropdown>
        </Nav>
      </Navbar>
    );
  }
}
