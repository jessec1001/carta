import React, { Component, HTMLProps } from "react";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCopy } from "@fortawesome/free-solid-svg-icons";

export interface CopyButtonProps extends HTMLProps<HTMLDivElement> {
  contents: string;
  text?: string;
}
export interface CopyButtonState {
  recentCopy: boolean;
}

export default class CopyButton extends Component<
  CopyButtonProps,
  CopyButtonState
> {
  constructor(props: CopyButtonProps) {
    super(props);
    this.state = {
      recentCopy: false,
    };
    this.handleClick = this.handleClick.bind(this);
  }

  handleClick(event: any) {
    // Copy the contents to the clipboard.
    navigator.clipboard.writeText(this.props.contents);
    this.setState({
      recentCopy: true,
    });
    setTimeout(() => {
      this.setState({
        recentCopy: false,
      });
    }, 1000);

    // Call the original on click handler.
    if (this.props.onClick) this.props.onClick(event);
  }

  render() {
    let { contents, text, ...rest } = this.props;

    // Assign some default text to the button if none is specified.
    text = text ?? (this.state.recentCopy ? "Copied" : "Copy");

    return (
      <div {...rest} onClick={this.handleClick}>
        {text} <FontAwesomeIcon icon={faCopy} />
      </div>
    );
  }
}
