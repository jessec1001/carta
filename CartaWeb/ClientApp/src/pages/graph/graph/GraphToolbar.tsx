import React, { Component } from "react";
import {
  Navbar,
  Nav,
  UncontrolledDropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
  NavbarProps,
} from "reactstrap";
import { GraphData } from "library/api/data/types";
import { GraphWorkflow } from "library/api/workflow/types";

import "./GraphToolbar.css";
import { MetaApi } from "library/api";
import { MetaTypeEntry } from "library/api/meta";
import { SchemaFormProps } from "components/form/schema/SchemaForm";

interface GraphToolbarProps extends NavbarProps {
  graph?: GraphData;

  onCreateForm?: (type: string, props: SchemaFormProps) => void;

  onOpenGraph?: () => void;
  onCloseGraph?: () => void;
  onDuplicateGraph?: () => void;

  onCreateWorkflow?: () => Promise<any>;
  onApplyWorkflow?: () => void;
}
interface GraphToolbarState {
  actors?: Record<string, Record<string, MetaTypeEntry>>;
  selectors?: Record<string, Record<string, MetaTypeEntry>>;
}

export class GraphToolbar extends Component<
  GraphToolbarProps,
  GraphToolbarState
> {
  constructor(props: GraphToolbarProps) {
    super(props);

    this.fetchMetadataAsync = this.fetchMetadataAsync.bind(this);

    this.handleSelector = this.handleSelector.bind(this);
    this.handleAction = this.handleAction.bind(this);

    this.handleExpand = this.handleExpand.bind(this);
    this.handleCollapse = this.handleCollapse.bind(this);

    this.handleOpenGraph = this.handleOpenGraph.bind(this);
    this.handleCloseGraph = this.handleCloseGraph.bind(this);
    this.handleDuplicateGraph = this.handleDuplicateGraph.bind(this);

    this.handleInvertSelection = this.handleInvertSelection.bind(this);

    this.handleCreateWorkflow = this.handleCreateWorkflow.bind(this);
    this.handleApplyWorkflow = this.handleApplyWorkflow.bind(this);
    this.handleClearWorkflow = this.handleClearWorkflow.bind(this);

    this.handleUndo = this.handleUndo.bind(this);
    this.handleRedo = this.handleRedo.bind(this);
    this.handleKeyboard = this.handleKeyboard.bind(this);

    this.fetchMetadataAsync();

    this.state = {};
  }

  private async fetchMetadataAsync() {
    // Fetch the actors and selectors existance data from the server.
    const actors = await MetaApi.getActorsAsync();
    const selectors = await MetaApi.getSelectorsAsync();

    // We will be storing the actors and selectors: first by group, second by key.
    const actorsByGroup: Record<string, Record<string, MetaTypeEntry>> = {};
    const selectorsByGroup: Record<string, Record<string, MetaTypeEntry>> = {};

    // Convert the actors structure to be sorted by group.
    Object.entries(actors).forEach(([key, entry]) => {
      // Check that the group has been created.
      const group = entry.group ?? "";
      if (!(group in actorsByGroup)) actorsByGroup[group] = {};

      // Add the entry to the group.
      actorsByGroup[group][key] = entry;
    });
    // Convert the selectors structure to be sorted by group.
    Object.entries(selectors).forEach(([key, entry]) => {
      // Check that the group has been created.
      const group = entry.group ?? "";
      if (!(group in selectorsByGroup)) selectorsByGroup[group] = {};

      // Add the entry to the group.
      selectorsByGroup[group][key] = entry;
    });

    // Set the state to reflect the obtained actors and selectors.
    this.setState({
      actors: actorsByGroup,
      selectors: selectorsByGroup,
    });
  }

  private async handleSelector(
    selectorType: string,
    useDefault?: boolean,
    name?: string
  ) {
    const callback = (selector: any) => {
      const { graph } = this.props;
      if (graph)
        graph.workflow.applySelector({
          ...selector,
          type: selectorType,
        });
    };

    const defaultValue = await MetaApi.getSelectorDefaultAsync({
      selector: selectorType,
    });
    delete defaultValue.type;
    if (selectorType === "descendants" || selectorType === "ancestors") {
      defaultValue.ids = this.props.graph?.selection ?? [];
    }
    if (useDefault) {
      callback(defaultValue);
    } else {
      const schema = await MetaApi.getSelectorSchemaAsync({
        selector: selectorType,
      });
      schema.title = schema.title ?? name;
      if (this.props.onCreateForm) {
        this.props.onCreateForm("Selection", {
          schema: schema,
          value: defaultValue,
          onSubmit: callback,
        });
      }
    }
  }
  private async handleAction(
    actorType: string,
    useDefault?: boolean,
    name?: string
  ) {
    const callback = (action: any) => {
      const { graph } = this.props;
      if (graph)
        graph.workflow.applyAction({
          ...action,
          type: actorType,
        });
    };

    const defaultValue = await MetaApi.getActorDefaultAsync({
      actor: actorType,
    });
    delete defaultValue.type;
    if (useDefault) {
      callback(defaultValue);
    } else {
      const schema = await MetaApi.getActorSchemaAsync({ actor: actorType });
      schema.title = schema.title ?? name;
      if (this.props.onCreateForm) {
        this.props.onCreateForm("Action", {
          schema: schema,
          value: defaultValue,
          onSubmit: callback,
        });
      }
    }
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

  handleInvertSelection() {
    const graph = this.props.graph;
    if (graph) {
      graph.workflow.invertSelector();
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
      this.handleSelector("none", true);
      event.preventDefault();
    } else if (event.ctrlKey && event.key.toLowerCase() === "a") {
      this.handleSelector("all", true);
      event.preventDefault();
    }

    if (event.ctrlKey && event.key === "[") {
      this.handleSelector("collapsed");
    } else if (event.key === "[") {
      this.handleExpand();
    }
    if (event.ctrlKey && event.key === "]") {
      this.handleSelector("expanded");
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
      onCreateForm,
      ...rest
    } = this.props;

    return (
      <Navbar light expand="md" {...rest}>
        <Nav navbar>
          {this.renderGraphMenu()}
          {this.renderWorkflowMenu()}
          {this.renderNavigationMenu()}
          {this.renderSelectionMenu()}
          {this.renderActionMenu()}
        </Nav>
      </Navbar>
    );
  }

  private renderGraphMenu() {
    return (
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
    );
  }
  private renderWorkflowMenu() {
    return (
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
    );
  }
  private renderNavigationMenu() {
    return (
      <UncontrolledDropdown nav inNavbar>
        <DropdownToggle nav>Navigation</DropdownToggle>
        <DropdownMenu>
          <DropdownItem onClick={this.handleExpand}>
            Expand Selected Vertices <span className="shortcut">[</span>
          </DropdownItem>
          <DropdownItem onClick={this.handleCollapse}>
            Collapse Selected Vertices <span className="shortcut">]</span>
          </DropdownItem>
        </DropdownMenu>
      </UncontrolledDropdown>
    );
  }
  private renderSelectionMenu() {
    const { selectors } = this.state;
    return (
      <UncontrolledDropdown nav inNavbar>
        <DropdownToggle nav>Selection</DropdownToggle>
        <DropdownMenu>
          <DropdownItem onClick={this.handleInvertSelection}>
            Invert Selection
          </DropdownItem>
          {selectors === undefined && <span className="mx-2">Loading...</span>}
          {selectors !== undefined &&
            this.renderGroupedMenu(selectors, this.handleSelector)}
        </DropdownMenu>
      </UncontrolledDropdown>
    );
  }
  private renderActionMenu() {
    const { actors } = this.state;
    return (
      <UncontrolledDropdown nav inNavbar>
        <DropdownToggle nav>Action</DropdownToggle>
        <DropdownMenu>
          {actors === undefined && <span className="mx-2">Loading...</span>}
          {actors !== undefined &&
            this.renderGroupedMenu(actors, this.handleAction)}
        </DropdownMenu>
      </UncontrolledDropdown>
    );
  }

  private renderGroupedMenu(
    groups: Record<string, Record<string, MetaTypeEntry>>,
    callback: (key: string, useDefault?: boolean, name?: string) => void
  ) {
    return Object.entries(groups).map(([group, entries]) => {
      return (
        <React.Fragment key={group}>
          {group !== "" && (
            <div className="toolbar-group-label" key={group}>
              {group}
            </div>
          )}
          {Object.entries(entries).map(([key, entry]) => {
            if (!entry.hidden)
              return (
                <DropdownItem
                  key={key}
                  onClick={() => callback(key, false, entry.name)}
                >
                  {entry.name}
                </DropdownItem>
              );
            else return null;
          })}
        </React.Fragment>
      );
    });
  }
}
