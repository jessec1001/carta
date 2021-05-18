import UserContext from "components/ui/user";
import { User } from "library/api/user/types";
import React, { Component } from "react";
import LabeledInput, { LabeledInputProps } from "./LabeledInput";

export interface UserInputProps extends LabeledInputProps {
  field: keyof User;
}

export default class UserInput extends Component<UserInputProps> {
  render() {
    const { field, children, ref, ...restProps } = this.props;
    return (
      <UserContext.Consumer>
        {(value) => {
          return (
            value.user !== null && (
              <LabeledInput value={value.user[field]} {...restProps}>
                {children}
              </LabeledInput>
            )
          );
        }}
      </UserContext.Consumer>
    );
  }
}