import React, { Component, CSSProperties, ReactElement } from "react";
import classNames from "classnames";

import { TabBar, TabButton } from ".";
import { TabProps } from "./Tab";

import "./TabPane.css";

export interface TabPaneProps {
  children?: ReactElement<TabProps> | ReactElement<TabProps>[];
  className?: string;
  style?: CSSProperties;

  onClose?: (index: number) => void;
}

export interface TabPaneState {
  selected: string | null;
}

export default class TabPane extends Component<TabPaneProps, TabPaneState> {
  static displayName = TabPane.name;

  constructor(props: TabPaneProps) {
    super(props);

    this.state = {
      selected: this.getSelectedTab(),
    };

    this.handleSelectTab = this.handleSelectTab.bind(this);
    this.handleCloseTab = this.handleCloseTab.bind(this);
  }

  componentDidUpdate(prevProps: TabPaneProps) {
    if (this.props.children !== prevProps.children) {
      this.setState({
        selected: this.getSelectedTab(),
      });
    }
  }

  /** Gets an array of the tabs passed to this component. */
  getTabs(): ReactElement<TabProps>[] {
    if (this.props.children) {
      if (Array.isArray(this.props.children)) {
        return this.props.children as ReactElement<TabProps>[];
      } else {
        return [this.props.children];
      }
    }
    return [];
  }
  /** Gets the label for the selected tab. Gets the first if many are selected. Returns null if no tab is selected. */
  getSelectedTab() {
    const tabs = this.getTabs();
    if (tabs.length > 0) {
      for (let k = 0; k < tabs.length; k++) {
        if (tabs[k].props.selected) return tabs[k].props.label;
        if (this.state && this.state.selected === tabs[k].props.label)
          return tabs[k].props.label;
      }
      return tabs[0].props.label;
    } else return null;
  }

  /** Handles when a tab is selected. */
  handleSelectTab(label: string) {
    if (this.state.selected !== label) {
      this.setState({
        selected: label,
      });
    }
  }
  /** Handles when a tab is closed. */
  handleCloseTab(label: string) {
    if (this.props.onClose) {
      this.props.onClose(
        this.getTabs()
          .map((tab) => tab.props.label)
          .indexOf(label)
      );
    }
  }

  render() {
    const tabs = this.getTabs();

    return (
      <div
        className={classNames("tab-pane", this.props.className)}
        style={this.props.style}
      >
        {/* Create a tab bar with buttons for each of the contained tabs. */}
        <TabBar>
          {tabs.map((tab: ReactElement<TabProps>) => {
            return (
              <TabButton
                key={tab.props.label}
                icon={tab.props.icon}
                label={tab.props.label}
                closable={!!tab.props.closable}
                selected={tab.props.label === this.state.selected}
                onSelect={() => this.handleSelectTab(tab.props.label)}
                onClose={() => this.handleCloseTab(tab.props.label)}
                onContextMenu={tab.props.onContextMenu}
              />
            );
          })}
        </TabBar>

        {/*
          Only display the tab that is selected.
          Notice that the other tabs are still rendered (in React) but are visually hidden.
        */}
        {tabs.map((tab) => (
          <div
            className={classNames("tab-content", {
              hidden: tab.props.label !== this.state.selected,
            })}
            key={tab.props.label}
          >
            {tab}
          </div>
        ))}
      </div>
    );
  }
}
