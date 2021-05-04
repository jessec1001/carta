import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./Section.css";

export interface SectionProps extends HTMLProps<HTMLDivElement> {
  title: string;
}

export default class Section extends Component<SectionProps> {
  render() {
    const { title, className, children, ...restProps } = this.props;
    return (
      <section className={classNames(className, `section`)} {...restProps}>
        <h2 className={`section-heading`}>{title}</h2>
        <div className={`section-content`}>{children}</div>
      </section>
    );
  }
}
