import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./SidebarLayout.css";

export interface SidebarLayoutProps extends HTMLProps<HTMLDivElement> {
  side: "left" | "right";
}

export default class SidebarLayout extends Component<SidebarLayoutProps> {
  render() {
    const { side, className, children, ...restProps } = this.props;

    return (
      <div
        className={classNames(
          className,
          `sidebar-layout`,
          `sidebar-layout-${side}`
        )}
        {...restProps}
      >
        {children}
      </div>
    );
  }
}
