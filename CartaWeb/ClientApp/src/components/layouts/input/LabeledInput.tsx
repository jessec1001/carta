import React, { Component, HTMLProps } from "react";
import classNames from "classnames";
import "./LabeledInput.css";

export interface LabeledInputProps extends HTMLProps<HTMLInputElement> {}

export default class LabeledInput extends Component<LabeledInputProps> {
  render() {
    const { id, className, children, ...restProps } = this.props;
    return (
      <div className={classNames(className, `labeled-input`)}>
        <label className={`labeled-input-label`} htmlFor={id}>
          {children}
        </label>
        <input id={id} className={`labeled-input-input`} {...restProps} />
      </div>
    );
  }
}
