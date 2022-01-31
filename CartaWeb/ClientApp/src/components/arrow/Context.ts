import { createContext, useContext } from "react";

/**
 * The type of value used for the {@link ArrowsContext}.
 * This is meant to provide the minimal functionality to use arrows.
 */
interface IArrowsContext {}
/**
 * Exposes the state of, and the actions performable on an {@link Arrows}.
 */
interface IArrows {}

/** The context used to expose information about the {@link Arrows} component. */
const ArrowsContext = createContext<IArrowsContext | undefined>(undefined);

/**
 * Returns an object that allows for determining and modifying the state of arrows.
 * @returns The context for arrows.
 */
const useArrows = (): IArrows => {
  // Grab the context if it is defined.
  // If not defined, raise an error because the rest of this hook will not work.
  const context = useContext(ArrowsContext);
  if (!context) {
    throw new Error("Arrows context must be used within an arrows component.");
  }

  return { ...context };
};

export default ArrowsContext;
export { useArrows };
export type { IArrowsContext, IArrows };
