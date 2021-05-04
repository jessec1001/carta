import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./Section.css";

export interface SubsectionProps extends HTMLProps<HTMLDivElement> {
  title: string;
}

export default class Subsection extends Component<SubsectionProps> {
  render() {
    const { title, className, children, ...restProps } = this.props;
    return (
      <section className={classNames(className, `subsection`)} {...restProps}>
        <h3 className={`subsection-heading`}>{title}</h3>
        <div className={`subsection-content`}>{children}</div>
      </section>
    );
  }
}
