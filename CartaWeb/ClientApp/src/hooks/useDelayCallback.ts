import { useCallback, useRef } from "react";

/**
 * Creates a callback function that when called, will only execute the function until after a certain period of time has
 * passed without the callback being called again.
 * @param callback The callback to delay.
 * @param delay The amount of delay (in milliseconds).
 * @param cancels Whether the callback should be canceled if it is called again before the delay has passed.
 * @returns A new callback function that delays its execution.
 */
const useDelayCallback = <T extends any[]>(
  callback: (...args: T) => void,
  delay: number,
  cancels: boolean = true
): ((...args: T) => void) => {
  // We construct a new callback function that will only process calls to it after some specified time has passed.
  const timeoutRef = useRef<number | null>(null);
  const delayedCallback = useCallback(
    (...args: T) => {
      // If the callback was previously scheduled, we cancel it.
      if (cancels && timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }

      // We schedule the callback to be called after the delay.
      timeoutRef.current = setTimeout(callback, delay, ...args);
    },
    [callback, cancels, delay]
  );

  return delayedCallback;
};

export default useDelayCallback;
