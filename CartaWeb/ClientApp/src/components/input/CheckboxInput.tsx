import { FunctionComponent, HTMLProps } from "react";
import { useControllableState } from "hooks";
import { Modify } from "types";
import classNames from "classnames";

import "components/form/form.css";

/** The props used for the {@link CheckboxInput} component. */
interface CheckboxInputProps {
  value?: boolean;
  onChange?: (value: boolean) => void;
}

/** A component that defines a checkbox boolean input. */
const CheckboxInput: FunctionComponent<
  Modify<HTMLProps<HTMLDivElement>, CheckboxInputProps>
> = ({ children, className, value, onChange, ...props }) => {
  // We need to allow this component to be optionally controllable because we are not using a native UI element.
  const [actualValue, setValue] = useControllableState(
    value ?? false,
    value,
    onChange
  );

  // TODO: Attempt to use native input element for accessibility.
  // return (
  //   <label className="form-checkbox">
  //     <input
  //       type="checkbox"
  //       className={className}
  //       checked={value}
  //       onChange={(event) => {
  //         if (onChange) onChange(event.target.checked);
  //       }}
  //       {...props}
  //     />
  //     <span className="checkmark" />
  //   </label>
  // );

  return (
    <div
      className={classNames("form-checkbox", { checked: value }, className)}
      onClick={(event) => setValue(!actualValue)}
      {...props}
    />
  );
};

// Export React component and props.
export default CheckboxInput;
export type { CheckboxInputProps };
