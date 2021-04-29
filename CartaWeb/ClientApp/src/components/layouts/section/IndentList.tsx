import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./IndentList.css";

export interface IndentListProps extends HTMLProps<HTMLUListElement> {}

export default class IndentList extends Component<IndentListProps> {
  render() {
    const { className, children, ...restProps } = this.props;
    return (
      <ul className={classNames(className, `indent-list`)} {...restProps}>
        {React.Children.map(children, (child, index) => (
          <li className={`indent-list-item`} key={index}>
            {child}
          </li>
        ))}
      </ul>
    );
  }
}
