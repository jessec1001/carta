import React, { Component, MouseEvent } from "react";
import queryString from "query-string";

import { RouteComponentProps } from "react-router-dom";
import { HeightScroll } from "../components/layouts/HeightScroll";
import { PropertyList } from "../components/shared/properties/PropertyList";
import { GraphVisualizer } from "../visualizers";
import { GraphToolbar } from "../components/shared/nav/GraphToolbar";
import { Property } from "../lib/types/graph";
import { SplitPane, Tab, TabPane } from "../ui/panes";
import GraphOpenForm from "../forms/GraphOpenForm";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faProjectDiagram, faCogs } from "@fortawesome/free-solid-svg-icons";

import "./GraphPage.css";
import ContextMenu from "../ui/menu/context/ContextMenu";

interface GraphProperties {
  source: string;
  resource: string;
  parameters?: Record<string, any>;
  index: number;
}

export interface GraphPageProps extends RouteComponentProps {}
export interface GraphPageState {
  tabs: GraphProperties[];
  properties: Property[];
  selection?: { type: string; [key: string]: any };

  tabContextMenuVisible: boolean;
  tabContextMenuPosition: { x: number; y: number };
  tabContextTabIndex?: number;
  nodeContextMenuVisible: boolean;
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

    this.request = (this.props.location.pathname + this.props.location.search)
      .replace(this.props.match.path, "")
      .substring(1);

    const parsedRequest = queryString.parseUrl(this.request);
    const requests: string[] = parsedRequest.url.split(",");

    const parameters = parsedRequest.query;
    this.parameters = parsedRequest.query;

    this.index = 0;
    this.state = {
      tabs: requests
        .filter((request) => request.length > 0)
        .map((request) => ({
          source: request.split("/")[0],
          resource: request.split("/")[1],
          parameters,
          index: this.index++,
        })),
      properties: [],

      tabContextMenuVisible: false,
      tabContextMenuPosition: { x: 0, y: 0 },
      nodeContextMenuVisible: false,
    };

    this.handlePropertiesChanges = this.handlePropertiesChanges.bind(this);
    this.handleSelect = this.handleSelect.bind(this);
    this.handleOpenGraph = this.handleOpenGraph.bind(this);
    this.handleCloseGraph = this.handleCloseGraph.bind(this);
    this.handleOpenTabContextMenu = this.handleOpenTabContextMenu.bind(this);
    this.handleCloseTabContextMenu = this.handleCloseTabContextMenu.bind(this);
  }

  handlePropertiesChanges(properties: Array<Property>) {
    this.setState({
      properties: properties,
    });
  }
  handleSelect(selector: any) {
    this.setState({
      selection: selector,
    });
  }

  handleOpenGraph(
    source: string,
    resource: string,
    parameters?: Record<string, any>
  ) {
    this.setState((state) => {
      const tabs = [
        ...state.tabs,
        { source, resource, parameters, index: this.index++ },
      ];

      this.props.history.push({
        pathname: `/graph/${tabs.map(
          (tab) => `${tab.source}/${tab.resource}`
        )}`,
      });

      return { tabs };
    });
  }
  handleCloseGraph(index: number) {
    this.setState((state) => {
      const tabs = [
        ...state.tabs.slice(0, index),
        ...state.tabs.slice(index + 1),
      ];

      this.props.history.push({
        pathname: `/graph/${tabs.map(
          (tab) => `${tab.source}/${tab.resource}`
        )}`,
      });

      return { tabs };
    });
  }

  handleOpenTabContextMenu(event: MouseEvent, index: number) {
    this.setState({
      tabContextMenuVisible: true,
      tabContextMenuPosition: { x: event.pageX, y: event.pageY },
      tabContextTabIndex: index,
    });
    event.preventDefault();
  }
  handleCloseTabContextMenu() {
    this.setState({
      tabContextMenuVisible: false,
    });
  }
  handleSelectTabContextMenu(key: string) {
    // switch (key) {
      // case "duplicate":
        // if (this.state.tabContextTabIndex) {
        //   const tab = this.state.tabs.find(tab => tab.index === this.state.tabContextTabIndex);
        //   if (!tab) return;
        //   // this.handleOpenGraph();
        // }
        // break;
    // }
  }

  render() {
    return (
      <SplitPane direction="horizontal">
        <div className="d-flex flex-column h-100">
          <GraphToolbar className="toolbar" onSelect={this.handleSelect} />
          <ContextMenu
            visible={this.state.tabContextMenuVisible}
            entries={[{ key: "duplicate", label: "Duplicate Graph" }]}
            position={this.state.tabContextMenuPosition}
            onExit={this.handleCloseTabContextMenu}
            onSelect={this.handleSelectTabContextMenu}
          />
          <TabPane onClose={this.handleCloseGraph}>
            {this.state.tabs.map((tab) => (
              <Tab
                key={tab.index}
                icon={<FontAwesomeIcon icon={faProjectDiagram} />}
                label={`${tab.source} - ${tab.resource}`}
                closable={true}
                onContextMenu={(event) =>
                  this.handleOpenTabContextMenu(event, tab.index)
                }
              >
                <GraphVisualizer
                  source={tab.source}
                  resource={tab.resource}
                  parameters={tab.parameters}
                  selector={this.state.selection}
                  onPropertiesChanged={this.handlePropertiesChanges}
                />
              </Tab>
            ))}
          </TabPane>
        </div>
        <TabPane className="side-tabs">
          <Tab icon={<FontAwesomeIcon icon={faCogs} />} label="Properties">
            <HeightScroll className="sidebar h-100">
              <PropertyList properties={this.state.properties} />
            </HeightScroll>
          </Tab>
          <Tab icon={<FontAwesomeIcon icon={faCogs} />} label="Open Graph">
            <GraphOpenForm
              parameters={this.parameters}
              onOpenGraph={this.handleOpenGraph}
            />
          </Tab>
        </TabPane>
      </SplitPane>
    );
  }
}
