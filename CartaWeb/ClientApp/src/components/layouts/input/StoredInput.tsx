import React, { Component, FormEvent, HTMLProps } from "react";
import classNames from "classnames";
import LabeledInput, { LabeledInputProps } from "./LabeledInput";

export interface StoredInputProps extends LabeledInputProps {
  field: string;
}
export interface StoredInputState {
  value: string;
}

export default class StoredInput extends Component<
  StoredInputProps,
  StoredInputState
> {
  constructor(props: StoredInputProps) {
    super(props);

    this.handleChange = this.handleChange.bind(this);
    this.handleUpdate = this.handleUpdate.bind(this);

    this.state = {
      value: localStorage.getItem(this.props.field) ?? "",
    };
  }

  handleChange(event: FormEvent<HTMLInputElement>) {
    this.setState({
      value: (event.target as HTMLInputElement).value,
    });
    if (this.props.onChange) this.props.onChange(event);
  }
  handleUpdate() {
    localStorage.setItem(this.props.field, this.state.value);
  }

  render() {
    const { field, children, ref, ...restProps } = this.props;
    return (
      <div style={{ display: "flex" }}>
        <LabeledInput
          style={{ flexGrow: 1 }}
          value={this.state.value}
          {...restProps}
          onChange={this.handleChange}
        >
          {children}
        </LabeledInput>
        <button className={`button bg-action`} onClick={this.handleUpdate}>
          Update
        </button>
      </div>
    );
  }
}
