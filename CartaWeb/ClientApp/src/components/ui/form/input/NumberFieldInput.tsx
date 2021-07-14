import { FunctionComponent, HTMLProps } from "react";
import { Modify } from "types";
import classNames from "classnames";

import "components/ui/form/form.css";

/** The props used for the {@link NumberFieldInput} component. */
interface NumberFieldInputProps {
  value?: number;
  onChange?: (value: number) => void;
}

/** A component that defines a text-based, number input. */
const NumberFieldInput: FunctionComponent<
  Omit<Modify<HTMLProps<HTMLInputElement>, NumberFieldInputProps>, "type">
> = ({ children, className, value, onChange, ...props }) => {
  // We need to cast NaN to a string if it is passed in.
  const inputValue = value === undefined || !isNaN(value) ? value : "NaN";

  // Simply render a basic input element with an extra class name.
  return (
    <input
      className={classNames("form-control", className)}
      type="number"
      value={inputValue}
      onChange={(event) => {
        if (onChange) onChange(event.target.valueAsNumber);
      }}
      {...props}
    />
  );
};

// Export React component and props.
export default NumberFieldInput;
export type { NumberFieldInputProps };
