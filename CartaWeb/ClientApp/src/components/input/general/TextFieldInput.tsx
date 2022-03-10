import { FunctionComponent, HTMLProps } from "react";
import { Modify } from "types";
import classNames from "classnames";
import { IBaseInputProps } from "components/input";

import "components/form/form.css";

/** The props used for the {@link TextFieldInput} component. */
interface TextFieldInputProps extends IBaseInputProps<string> {
  /** Whether the text represents a password and should be obfuscated. */
  password?: boolean;
  /** Whether all of the text should be selected when the text field is focused. */
  autoSelect?: boolean;
}

/** A component that defines a single-line text input. */
const TextFieldInput: FunctionComponent<
  Omit<Modify<HTMLProps<HTMLInputElement>, TextFieldInputProps>, "type">
> = ({
  children,
  className,
  password,
  autoSelect,
  value,
  onChange,
  onFocus,
  ...props
}) => {
  // Simply render a basic input element with an extra class name.
  return (
    <input
      className={classNames("form-control", className)}
      type={password ? "password" : "text"}
      value={value}
      onChange={(event) => {
        if (onChange) onChange(event.target.value);
      }}
      onFocus={(event) => {
        if (onFocus) onFocus(event);
        if (autoSelect) event.target.select();
      }}
      {...props}
    />
  );
};

// Export React component and props.
export default TextFieldInput;
export type { TextFieldInputProps };
