import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./Mainbar.css";

export interface MainbarProps extends HTMLProps<HTMLDivElement> {}

export default class Mainbar extends Component<MainbarProps> {
  render() {
    const { className, children, ...restProps } = this.props;
    return (
      <main className={classNames(className, `mainbar`)} {...restProps}>
        {children}
      </main>
    );
  }
}
