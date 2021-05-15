import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./Link.css";

export interface LinkProps extends HTMLProps<HTMLAnchorElement> {}

export default class Link extends Component<LinkProps> {
  render() {
    const { className, children, ...restProps } = this.props;
    return (
      <a className={classNames(className, `link`)} {...restProps}>
        {children}
      </a>
    );
  }
}
