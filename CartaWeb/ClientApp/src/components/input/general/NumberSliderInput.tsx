import { FunctionComponent, HTMLProps } from "react";
import { useControllableState } from "hooks";
import { Modify } from "types";
import classNames from "classnames";

import "components/form/form.css";

/**
 * Computes the progress of the slider as a number in [0, 1].
 * Returns zero if any property of the slider is not defined as a number.
 * @param min The minimum slider value.
 * @param max The maximum slider value.
 * @param value The current slider value.
 */
const computeProgress = (min: any, max: any, value: any) => {
  if (
    typeof min !== "number" ||
    typeof max !== "number" ||
    typeof value !== "number"
  )
    return 0;
  else return (value - min) / (max - min);
};

/** The props used for the {@link NumberSliderInput} component. */
interface NumberSliderInputProps {
  value?: number;
  onChange?: (value: number) => void;
}

/** A component that defines a slider-based, number input. */
const NumberSliderInput: FunctionComponent<
  Omit<Modify<HTMLProps<HTMLInputElement>, NumberSliderInputProps>, "type">
> = ({ children, className, min, max, value, onChange, ...props }) => {
  // We need to allow this component to be optionally controllable.
  const [actualValue, setValue] = useControllableState(
    value ?? 0,
    value,
    onChange
  );

  // We need the slider's position.
  const progress = computeProgress(min, max, actualValue);

  // We need to wrap everything in some containers to do the following:
  // 1. Make space for the minimum and maximum values.
  // 2. Make the slider stretch across the entire parent.
  // 3. Make the slider value rest on top of the slider thumb.
  // Notice how we set the '--progress' CSS variable. This is used for most of the layout computations.
  return (
    <div
      className={classNames("form-slider-container", className)}
      style={{
        ["--progress" as any]: progress,
      }}
    >
      {min}
      <div className="form-slider-box">
        <input
          className="form-slider"
          type="range"
          min={min}
          max={max}
          value={actualValue}
          onChange={(event) => setValue(event.target.valueAsNumber)}
          {...props}
        />
        <div className="form-slider-label">{actualValue}</div>
      </div>
      {max}
    </div>
  );
};

// Export React component and props.
export default NumberSliderInput;
export type { NumberSliderInputProps };
