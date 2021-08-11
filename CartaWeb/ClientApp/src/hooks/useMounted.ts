import { RefObject, useEffect, useRef } from "react";

/**
 * Creates a reference to whether the current component is mounted or not. Useful for canceling asynchronous state-
 * setting operations.
 * @returns A reference to the mounted state.
 */
const useMounted = (): RefObject<boolean> => {
  // We initialize the mounted reference to true to indicate that the component is currently mounted.
  const mountedRef = useRef<boolean>(true);

  // When the component is destroyed, we set the value of the mounted reference to false.
  useEffect(() => {
    return () => {
      mountedRef.current = false;
    };
  }, []);

  return mountedRef;
};

export default useMounted;
