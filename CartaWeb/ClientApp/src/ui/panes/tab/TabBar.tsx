import React, { Component, CSSProperties } from "react";
import classNames from "classnames";

import "./TabBar.css";

export interface TabBarProps {
  className?: string;
  style?: CSSProperties;
}

export default class TabBar extends Component<TabBarProps> {
  static displayName = TabBar.name;

  render() {
    return (
      <div className={classNames("tab-bar", this.props.className)}>
        {this.props.children}
      </div>
    );
  }
}
