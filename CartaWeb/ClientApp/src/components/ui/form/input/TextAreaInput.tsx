import { FunctionComponent, HTMLProps } from "react";
import { Modify } from "types";
import classNames from "classnames";

import "components/ui/form/form.css";

/** The props used for the {@link TextAreaInput} component. */
interface TextAreaInputProps {
  value?: string;
  onChange?: (value: string) => void;
}

/** A component that defines a multiple-line text input. */
const TextAreaInput: FunctionComponent<
  Modify<HTMLProps<HTMLTextAreaElement>, TextAreaInputProps>
> = ({ children, className, value, onChange, ...props }) => {
  return (
    <textarea
      className={classNames("form-control", className)}
      value={value}
      onChange={(event) => {
        if (onChange) onChange(event.target.value);
      }}
      {...props}
    />
  );
};

// Export React component and props.
export default TextAreaInput;
export type { TextAreaInputProps };
