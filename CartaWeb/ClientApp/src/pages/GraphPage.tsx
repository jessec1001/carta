import React, { Component } from "react";
import queryString from "query-string";

import { RouteComponentProps } from "react-router-dom";
import { HeightScroll } from "../components/layouts/HeightScroll";
import { PropertyList } from "../components/shared/properties/PropertyList";
import { GraphVisualizer } from "../visualizers";
import { GraphToolbar } from "../components/shared/nav/GraphToolbar";
import { Property } from "../lib/types/graph";
import { SplitPane, Tab, TabPane } from "../ui/panes";
import GraphOpenForm from "../forms/GraphOpenForm";
import { GraphData, GraphWorkflow } from "../lib/graph";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faProjectDiagram, faCogs } from "@fortawesome/free-solid-svg-icons";

import "./GraphPage.css";
import WorkflowCreateForm from "../forms/workflow/WorkflowCreateForm";
import WorkflowApplyForm from "../forms/workflow/WorkflowApplyForm";

export interface GraphPageProps extends RouteComponentProps {}
export interface GraphPageState {
  graphs: { index: number; data: GraphData }[];
  properties: Property[];

  workflowCreator: ((workflow: any) => void) | null;
  workflowApplying?: boolean;

  selectedGraph?: number;
  openingGraph: boolean;
}

export default class GraphPage extends Component<
  GraphPageProps,
  GraphPageState
> {
  request: string;
  index: number;
  parameters: Record<string, any>;

  constructor(props: GraphPageProps) {
    super(props);

    this.handlePropertiesChanges = this.handlePropertiesChanges.bind(this);
    this.handleCreateWorkflow = this.handleCreateWorkflow.bind(this);
    this.handleApplyWorkflow = this.handleApplyWorkflow.bind(this);
    this.handleOpenGraph = this.handleOpenGraph.bind(this);
    this.handleCloseGraph = this.handleCloseGraph.bind(this);
    this.handleSelectGraph = this.handleSelectGraph.bind(this);
    this.handleRequestOpenGraph = this.handleRequestOpenGraph.bind(this);
    this.handleRequestCloseGraph = this.handleRequestCloseGraph.bind(this);
    this.handleRequestDuplicateGraph = this.handleRequestDuplicateGraph.bind(
      this
    );

    this.request = (this.props.location.pathname + this.props.location.search)
      .replace(this.props.match.path, "")
      .substring(1);

    const parsedRequest = queryString.parseUrl(this.request);
    const requests: string[] = parsedRequest.url
      .split(",")
      .filter((request) => request.length > 0);

    const parameters = parsedRequest.query;
    this.parameters = parsedRequest.query;
    this.index = 0;

    const graphs = requests.map((request) => {
      const source = request.split("/")[0];
      const resource = request.split("/")[1];
      const index = this.index++;

      return {
        index,
        data: new GraphData(
          source,
          resource,
          new GraphWorkflow(undefined, this.handleCreateWorkflow),
          parameters
        ),
      };
    });

    this.state = {
      graphs: graphs,
      workflowCreator: null,
      openingGraph: graphs.length === 0,
      selectedGraph: graphs.length === 0 ? -1 : requests.length - 1,
      properties: [],
    };
  }

  handlePropertiesChanges(properties: Array<Property>) {
    this.setState({
      properties: properties,
    });
  }

  addGraph(graph: GraphData) {
    this.setState((state) => {
      const graphs = [
        ...state.graphs,

        {
          index: this.index++,
          data: graph,
        },
      ];

      this.props.history.push({
        pathname: `/graph/${graphs.map(
          (graph) => `${graph.data._source}/${graph.data._resource}`
        )}`,
      });

      return { graphs, selectedGraph: graphs.length - 1 };
    });
  }

  handleCreateWorkflow(): Promise<any> {
    return new Promise((resolve) => {
      this.setState({
        workflowCreator: (workflow: any) => {
          resolve(workflow);
          this.setState({
            workflowCreator: null,
          });
        },
      });
    });
  }
  handleApplyWorkflow() {
    this.setState({
      workflowApplying: true,
    });
  }

  handleOpenGraph(
    source: string,
    resource: string,
    parameters?: Record<string, any>
  ) {
    this.addGraph(
      new GraphData(
        source,
        resource,
        new GraphWorkflow(undefined, this.handleCreateWorkflow),
        parameters
      )
    );

    this.setState({ openingGraph: false });
  }
  handleCloseGraph(index: number) {
    if (index === this.state.graphs.length) {
      this.setState((state) => {
        let selectedGraph = state.selectedGraph;
        if (selectedGraph === -1) {
          if (state.graphs.length > 0) selectedGraph = state.graphs.length - 1;
          else selectedGraph = undefined;
        }
        return { openingGraph: false, selectedGraph };
      });
    } else {
      this.setState((state) => {
        let selectedGraph = state.selectedGraph;
        if (selectedGraph === index) {
          if (state.graphs.length > 1) selectedGraph = Math.max(0, index - 1);
          else if (state.openingGraph) selectedGraph = -1;
          else selectedGraph = undefined;
        }
        const graphs = [
          ...state.graphs.slice(0, index),
          ...state.graphs.slice(index + 1),
        ];

        this.props.history.push({
          pathname: `/graph/${graphs.map(
            (graph) => `${graph.data._source}/${graph.data._resource}`
          )}`,
        });

        return { graphs, selectedGraph };
      });
    }
  }
  handleSelectGraph(index: number) {
    this.setState({
      selectedGraph: index === this.state.graphs.length ? -1 : index,
    });
  }

  handleRequestOpenGraph() {
    this.setState({ openingGraph: true, selectedGraph: -1 });
  }
  handleRequestCloseGraph() {
    if (this.state.selectedGraph === undefined) return;
    if (this.state.selectedGraph === -1) return;
    this.handleCloseGraph(this.state.selectedGraph);
  }
  handleRequestDuplicateGraph() {
    const selectedGraph = this.state.selectedGraph;
    const graphs = this.state.graphs;
    if (selectedGraph !== undefined && selectedGraph !== -1) {
      this.addGraph(graphs[selectedGraph].data.duplicate());
    }
  }

  render() {
    let graph: GraphData | undefined;
    if (
      this.state.selectedGraph !== undefined &&
      this.state.selectedGraph !== -1
    )
      graph = this.state.graphs[this.state.selectedGraph].data;

    return (
      <SplitPane direction="horizontal" initialSizes={[3, 1]}>
        <div className="d-flex flex-column h-100">
          {this.state.workflowCreator && (
            <WorkflowCreateForm
              open
              onContinue={this.state.workflowCreator}
              onCancel={this.state.workflowCreator}
            />
          )}
          {this.state.workflowApplying && (
            <WorkflowApplyForm
              open
              onApply={(workflowId: number) => {
                if (graph) {
                  const workflow = new GraphWorkflow(
                    workflowId,
                    this.handleCreateWorkflow
                  );
                  graph.setWorkflow(workflow);
                }
                this.setState({ workflowApplying: false });
              }}
              onCancel={() => {
                this.setState({ workflowApplying: false });
              }}
            />
          )}
          <GraphToolbar
            className="toolbar"
            graph={graph}
            onOpenGraph={this.handleRequestOpenGraph}
            onCloseGraph={this.handleRequestCloseGraph}
            onDuplicateGraph={this.handleRequestDuplicateGraph}
            onCreateWorkflow={this.handleCreateWorkflow}
            onApplyWorkflow={this.handleApplyWorkflow}
          />
          <TabPane
            onClose={this.handleCloseGraph}
            onSelect={this.handleSelectGraph}
          >
            {this.state.graphs.map((graph, index) => (
              <Tab
                key={graph.index}
                icon={<FontAwesomeIcon icon={faProjectDiagram} />}
                label={`${graph.data._source} - ${graph.data._resource}`}
                closable
                selected={this.state.selectedGraph === index}
              >
                <GraphVisualizer
                  graph={graph.data}
                  onPropertiesChanged={this.handlePropertiesChanges}
                />
              </Tab>
            ))}
            {this.state.openingGraph && (
              <Tab
                key={-1}
                icon={<FontAwesomeIcon icon={faCogs} />}
                label="Open Graph"
                closable
                selected={this.state.selectedGraph === -1}
              >
                <GraphOpenForm
                  parameters={this.parameters}
                  onOpenGraph={this.handleOpenGraph}
                />
              </Tab>
            )}
          </TabPane>
        </div>
        <TabPane className="side-tabs">
          <Tab icon={<FontAwesomeIcon icon={faCogs} />} label="Properties">
            <HeightScroll className="sidepane h-100">
              <PropertyList properties={this.state.properties} />
            </HeightScroll>
          </Tab>
        </TabPane>
      </SplitPane>
    );
  }
}
