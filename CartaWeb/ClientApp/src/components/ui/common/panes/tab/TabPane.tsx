import React, { Component, CSSProperties, ReactElement } from "react";
import classNames from "classnames";

import { TabBar, TabButton } from ".";
import { TabProps } from "./Tab";

import "./TabPane.css";

type TabType =
  | ReactElement<TabProps>
  | ReactElement<TabProps>[]
  | boolean
  | null;

export interface TabPaneProps {
  children?: TabType | TabType[];
  className?: string;
  style?: CSSProperties;

  onSelect?: (index: number) => void;
  onClose?: (index: number) => void;
}

export interface TabPaneState {
  selected?: string | number | null;
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
      return React.Children.toArray(
        this.props.children
      ) as ReactElement<TabProps>[];
    }
    return [];
  }
  /** Gets the label for the selected tab. Gets the first if many are selected. Returns null if no tab is selected. */
  getSelectedTab() {
    const tabs = this.getTabs();
    if (tabs.length > 0) {
      for (let k = 0; k < tabs.length; k++) {
        if (tabs[k].props.selected) return tabs[k].key;
      }
      for (let k = 0; k < tabs.length; k++) {
        if (this.state && this.state.selected === tabs[k].key)
          return tabs[k].key;
      }
      return tabs[0].key;
    } else return undefined;
  }

  /** Handles when a tab is selected. */
  handleSelectTab(key: string | number | null) {
    if (this.state.selected !== key) {
      this.setState({
        selected: key,
      });
      if (this.props.onSelect) {
        this.props.onSelect(this.getTabs().findIndex((tab) => tab.key === key));
      }
    }
  }
  /** Handles when a tab is closed. */
  handleCloseTab(key: string | number | null) {
    if (this.props.onClose) {
      this.props.onClose(this.getTabs().findIndex((tab) => tab.key === key));
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
                key={tab.key}
                icon={tab.props.icon}
                label={tab.props.label}
                closable={tab.props.closable === true}
                selected={tab.key === this.state.selected}
                onSelect={() => this.handleSelectTab(tab.key)}
                onClose={() => this.handleCloseTab(tab.key)}
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
              hidden: tab.key !== this.state.selected,
            })}
            key={tab.key}
          >
            {tab}
          </div>
        ))}
      </div>
    );
  }
}
