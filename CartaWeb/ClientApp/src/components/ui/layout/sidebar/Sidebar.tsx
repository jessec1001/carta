import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./Sidebar.css";

export interface SidebarProps extends HTMLProps<HTMLDivElement> {}

export default class Sidebar extends Component<SidebarProps> {
  render() {
    const { className, children, ...restProps } = this.props;
    return (
      <aside className={classNames(className, `sidebar`)} {...restProps}>
        {children}
      </aside>
    );
  }
}
