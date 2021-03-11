import React, { Component, MouseEvent, RefObject } from "react";
import classNames from "classnames";

import "./SplitGutter.css";

export interface SplitGutterProps {
  className?: string;
  innerRef?: RefObject<HTMLDivElement>;

  direction: "vertical" | "horizontal";

  onMouseDown?: (event: MouseEvent) => void;
  onMouseUp?: (event: MouseEvent) => void;
}

export default class SplitGutter extends Component<SplitGutterProps> {
  render() {
    return (
      <div
        className={classNames(
          "split-gutter",
          this.props.direction,
          this.props.className
        )}
        onMouseDown={this.props.onMouseDown}
        onMouseUp={this.props.onMouseUp}
        ref={this.props.innerRef}
      ></div>
    );
  }
}
