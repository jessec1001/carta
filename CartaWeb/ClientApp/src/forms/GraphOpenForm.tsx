import React, { Component } from "react";
import { Nav, NavItem, NavLink } from "reactstrap";
import { HeightScroll } from "../components/layouts/HeightScroll";
import { dataGetSources, dataGetResources } from "../lib/carta";

import "./GraphOpenForm.css";

export interface GraphOpenFormProps {
  parameters?: Record<string, any>;

  onOpenGraph?: (
    source: string,
    resource: string,
    parameters?: Record<string, any>
  ) => void;
}
export interface GraphOpenFormState {
  resources?: Record<string, string[] | null>;
}

export default class GraphOpenForm extends Component<
  GraphOpenFormProps,
  GraphOpenFormState
> {
  static displayName = GraphOpenForm.name;

  constructor(props: {}) {
    super(props);

    this.state = {};
    this.fetchSources();
  }

  handleOpenGraph(source: string, resource: string) {
    if (this.props.onOpenGraph) {
      this.props.onOpenGraph(source, resource, this.props.parameters);
    }
  }

  render() {
    return (
      <HeightScroll className="form-graphOpen">
        {this.state.resources &&
          Object.keys(this.state.resources).map((source) => (
            <section key={source}>
              <h3 className="lead">{source}</h3>
              {this.state.resources![source] && (
                <Nav vertical>
                  {this.state.resources![source]?.map((resource) => (
                    <NavItem key={resource}>
                      <NavLink
                        href="#"
                        onClick={() => this.handleOpenGraph(source, resource)}
                      >
                        {resource}
                      </NavLink>
                    </NavItem>
                  ))}
                </Nav>
              )}
              {!this.state.resources![source] && <p>Loading</p>}
            </section>
          ))}
        {!this.state.resources && <h3>Loading</h3>}
      </HeightScroll>
    );
  }

  async fetchSources() {
    const sources = await dataGetSources(this.props.parameters);
    sources.forEach((source) => this.fetchResources(source));
    this.setState((state) => {
      const resources = { ...state.resources };
      sources.forEach((source) => (resources[source] = null));
      return { resources };
    });
  }
  async fetchResources(source: string) {
    const resources = await dataGetResources(source, this.props.parameters);
    if (Array.isArray(resources)) {
      this.setState((state) => ({
        resources: { ...state.resources, [source]: resources },
      }));
    }
  }
}
