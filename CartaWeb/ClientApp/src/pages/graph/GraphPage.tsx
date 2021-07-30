import React, { Component } from "react";
import queryString from "query-string";

import { RouteComponentProps } from "react-router-dom";
import { HeightScroll } from "components/ui/layout/scroll/HeightScroll";
import { PropertyList } from "components/visualizations/graph/PropertyList";
import { GraphVisualizer } from "components/visualizations";
import { GraphToolbar } from "./graph/GraphToolbar";
import { Property } from "library/api/data";
import { SplitPane, Tab, TabPane } from "components/common/panes";
import GraphOpenForm from "./graph/GraphOpenForm";
import { GraphData } from "library/api/data/types";
import { GraphWorkflow } from "library/api/workflow/types";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faProjectDiagram,
  faCogs,
  faBolt,
} from "@fortawesome/free-solid-svg-icons";

import WorkflowCreateForm from "./workflow/WorkflowCreateForm";
import WorkflowApplyForm from "./workflow/WorkflowApplyForm";
import SchemaForm, { SchemaFormProps } from "components/form/schema/SchemaForm";

export interface GraphPageProps extends RouteComponentProps {}
export interface GraphPageState {
  graphs: { index: number; data: GraphData }[];
  properties: Property[];

  workflowCreator: ((workflow: any) => void) | null;
  workflowApplying?: boolean;

  formType: string;
  formOpen: boolean;
  formValue?: any;
  formProps?: SchemaFormProps;

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

    this.handleCreateForm = this.handleCreateForm.bind(this);
    this.handlePropertiesChanges = this.handlePropertiesChanges.bind(this);
    this.handleCreateWorkflow = this.handleCreateWorkflow.bind(this);
    this.handleApplyWorkflow = this.handleApplyWorkflow.bind(this);
    this.handleOpenGraph = this.handleOpenGraph.bind(this);
    this.handleCloseGraph = this.handleCloseGraph.bind(this);
    this.handleSelectGraph = this.handleSelectGraph.bind(this);
    this.handleRequestOpenGraph = this.handleRequestOpenGraph.bind(this);
    this.handleRequestCloseGraph = this.handleRequestCloseGraph.bind(this);
    this.handleRequestDuplicateGraph =
      this.handleRequestDuplicateGraph.bind(this);

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
      formType: "",
      formOpen: false,
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

  handleCreateForm(type: string, props: SchemaFormProps) {
    let { onSubmit } = props;
    if (!onSubmit) onSubmit = () => {};
    const handleSubmit = (value: any) => {
      this.setState({
        formValue: undefined,
        formOpen: false,
      });
      onSubmit!(value);
    };

    this.setState({
      formType: type,
      formValue: props.value,
      formOpen: true,
      formProps: { ...props, onSubmit: handleSubmit },
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
    this.setState((state) => {
      if (index >= state.graphs.length) {
        if (state.graphs.length === 0) {
          return {
            selectedGraph: state.openingGraph ? -1 : undefined,
          };
        } else {
          return {
            selectedGraph: state.graphs.length - 1,
          };
        }
      } else {
        return {
          selectedGraph: index,
        };
      }
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
              onApply={(workflowId: string) => {
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
            onCreateForm={this.handleCreateForm}
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
          {this.state.formOpen && (
            <Tab
              icon={<FontAwesomeIcon icon={faBolt} />}
              label={this.state.formType}
              selected
            >
              {this.state.formProps && (
                <HeightScroll className="sidepane h-100">
                  <SchemaForm
                    {...(this.state.formProps as any)}
                    value={this.state.formValue}
                    onChange={(value) => this.setState({ formValue: value })}
                    onCancel={() => this.setState({ formOpen: false })}
                    submitText={"Apply"}
                    cancelText={"Cancel"}
                    cancelable
                  />
                </HeightScroll>
              )}
            </Tab>
          )}
        </TabPane>
      </SplitPane>
    );
  }
}
