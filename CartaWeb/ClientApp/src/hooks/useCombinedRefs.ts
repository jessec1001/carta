import React, { useEffect, useRef } from "react";

/**
 * Combines multiple refs together into a single ref that can be used as a substitute for multiple refs.
 * @param refs The multiple refs to combine.
 * @returns The combined ref.
 */
const useCombinedRefs = <T>(
  ...refs: React.RefObject<T>[]
): React.RefObject<T> => {
  const targetRef = useRef<T>(null);

  useEffect(() => {
    refs.forEach((ref) => {
      if (!ref) return;

      if (typeof ref === "function") {
        (ref as (value: T | null) => {})(targetRef.current);
      } else {
        (ref.current as any) = targetRef.current;
      }
    });
  }, [refs]);

  return targetRef;
};

export default useCombinedRefs;
