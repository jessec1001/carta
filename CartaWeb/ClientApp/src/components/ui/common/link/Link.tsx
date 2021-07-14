import { Component, HTMLProps } from "react";
import { Link as RouterLink } from "react-router-dom";
import classNames from "classnames";
import "./Link.css";

export interface LinkProps extends HTMLProps<HTMLAnchorElement> {}

export default class Link extends Component<LinkProps> {
  render() {
    const { className, children, href } = this.props;
    return (
      <RouterLink className={classNames(className, `link`)} to={href ?? "/"}>
        {children}
      </RouterLink>
    );
  }
}
