import { Dispatch, SetStateAction, useCallback, useState } from "react";

/**
 * Returns an optionally controlled stateful value, and a function to update it.
 * @param initialValue The initial value to be assigned to the state.
 * @param propValue The current controlled value to be used in place of the stateful value.
 * @param handleChangeValue The handler to call when the stateful value has been changed or requested change.
 */
const useControllableState = <T>(
  initialValue: T | (() => T),
  propValue?: T,
  handleChangeValue?: (nextValue: T) => void
): [T, Dispatch<SetStateAction<T>>] => {
  // If the prop value is undefined, we assume that the component should be uncontrolled.
  const controlled = propValue !== undefined;

  // Find the value from the props, if specified, or the state otherwise.
  const [stateValue, setStateValue] = useState(initialValue);
  const actualValue = (controlled ? propValue : stateValue) as T;

  // Wrap the set state dispatch function so that it calls the change handler
  // and only sets a new state if uncontrolled to avoid re-rendering unecessarily.
  const handleValue: Dispatch<SetStateAction<T>> = useCallback(
    (value: T | ((prevValue: T) => T)) => {
      setStateValue((prevValue: T) => {
        // Compute the next value depending on whether we are using a callback function or a simple value.
        const nextValue =
          typeof value === "function"
            ? (value as (prevValue: T) => T)(prevValue)
            : value;

        // Notify the change handler.
        if (handleChangeValue) handleChangeValue(nextValue);

        // If uncontrolled, we change this component state.
        return controlled ? prevValue : nextValue;
      });
    },
    [controlled, handleChangeValue]
  );

  // Return the same signature as the use state React hook.
  return [actualValue, handleValue];
};

export default useControllableState;
