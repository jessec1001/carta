import { useEffect, useRef } from "react";

/**
 * Returns the previous state of a tracked value.
 * @param value The value to track the previous state of.
 * @returns The previous state of the value.
 */
const usePrevious = <T>(value: T): T => {
  // Since the effect hook will be called after the execution of this hook,
  // we can safely use it to update the value postfactor.
  const ref = useRef<T>(value);
  useEffect(() => {
    ref.current = value;
  });
  return ref.current;
};

export default usePrevious;
