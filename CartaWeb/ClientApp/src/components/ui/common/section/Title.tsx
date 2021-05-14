import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./Title.css";

export interface TitleProps extends HTMLProps<HTMLHeadingElement> {}

export default class Title extends Component<TitleProps> {
  render() {
    const { children, className, ...restProps } = this.props;
    return (
      <h1 className={classNames(className, `title`)} {...restProps}>
        {children}
      </h1>
    );
  }
}
