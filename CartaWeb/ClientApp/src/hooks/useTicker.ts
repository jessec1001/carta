import { useCallback, useEffect, useState } from "react";

/**
 * Picks a random item from a list on a regular interval.
 * @param items The list of items.
 * @param active Whether the ticker is active.
 * @param delay The delay between ticks (in milliseconds).
 * @returns The current item.
 */
const useTicker = <T = string>(items: T[], active: boolean, delay?: number) => {
  // We pick a new random item from the list when we initialize and when we tick.
  const itemPicker = useCallback(
    () => items[Math.floor(Math.random() * items.length)],
    [items]
  );
  const [item, setItem] = useState<T>(itemPicker);

  // We execute the ticker function on a delay with a default of 1000ms.
  delay = delay || 1000;
  useEffect(() => {
    const handleTick = () => setItem(itemPicker);
    if (active) {
      const interval = setInterval(handleTick, delay);
      return () => clearInterval(interval);
    }
  }, [active, delay, itemPicker]);

  // Yield the current item.
  return item;
};

export default useTicker;
