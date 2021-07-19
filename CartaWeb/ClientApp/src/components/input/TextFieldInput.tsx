import { FunctionComponent, HTMLProps } from "react";
import { Modify } from "types";
import classNames from "classnames";

import "components/form/form.css";

/** The props used for the {@link TextFieldInput} component. */
interface TextFieldInputProps {
  password?: boolean;

  value?: string;
  onChange?: (value: string) => void;
}

/** A component that defines a single-line text input. */
const TextFieldInput: FunctionComponent<
  Omit<Modify<HTMLProps<HTMLInputElement>, TextFieldInputProps>, "type">
> = ({ children, className, password, value, onChange, ...props }) => {
  // Simply render a basic input element with an extra class name.
  return (
    <input
      className={classNames("form-control", className)}
      type={password ? "password" : "text"}
      value={value}
      onChange={(event) => {
        if (onChange) onChange(event.target.value);
      }}
      {...props}
    />
  );
};

// Export React component and props.
export default TextFieldInput;
export type { TextFieldInputProps };
