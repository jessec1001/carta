import React, { Component, CSSProperties } from "react";
import classNames from "classnames";

import "./TabButton.css";

export interface TabButtonProps {
  className?: string;
  style?: CSSProperties;

  icon?: JSX.Element;
  label: string;
  closable: boolean;
  selected: boolean;

  onSelect?: () => void;
  onClose?: () => void;
  onContextMenu?: (event: any) => void;
}

export default class TabButton extends Component<TabButtonProps> {
  static displayName = TabButton.name;

  render() {
    return (
      <button
        className={classNames("tab-button", this.props.className, {
          selected: this.props.selected,
        })}
        style={this.props.style}
        onClick={this.props.onSelect}
        onContextMenu={this.props.onContextMenu}
      >
        {this.props.icon && (
          <span className={classNames("tab-icon")}>{this.props.icon}</span>
        )}
        <span className={classNames("tab-label")}>{this.props.label}</span>
        {this.props.closable && (
          <span
            className={classNames("tab-close")}
            onClick={this.props.onClose}
          >
            &times;
          </span>
        )}
      </button>
    );
  }
}
