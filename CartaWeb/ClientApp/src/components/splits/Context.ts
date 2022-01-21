import { createContext, useContext } from "react";

/**
 * The type of value used for the {@link SplitsContext}.
 * This is meant to provide the minimal functionality to use splits.
 * Additional features are defined in {@link ISplitsActions}.
 */
interface ISplitsContext {
  /** The direction of the splits. */
  direction: "horizontal" | "vertical";

  /** Gets the size ratio of a panel specified by its identifier. */
  getSize: (id: string | number) => number;
  /** Sets the size ratio of a panel specified by its identifier. */
  setSize: (id: string | number, size: number | null) => void;
  /** Unsets the size ratio of a panel specified by its identifier. */
  unsetSize: (id: string | number) => void;
}
/**
 * Defines actions that can be performed on the {@link Splits} component.
 * Extends the functionality of the {@link ISplitsContext} interface.
 */
interface ISplitsActions {
  /** Gets the size ratio of a panel specified by its identifier. */
  get: (id: string | number) => number;
  /** Sets the size ratio of a panel specified by its identifier. */
  set: (id: string | number, size: number | null) => void;
  /** Unsets the size ratio of a panel specified by its identifier. */
  unset: (id: string | number) => void;
}
/**
 * Exposes the state of, and the actions performable on a {@link Splits}.
 */
interface ISplits {
  /** The direction of the splits. */
  direction: "horizontal" | "vertical";
  /** Actions that can be performed on the splits. */
  actions: ISplitsActions;
}

/** The context used to information about the {@link Splits} component. */
const SplitsContext = createContext<ISplitsContext | undefined>(undefined);

/**
 * Returns an object that allows for determining the state of splits along with actions that allow changing the state of
 * the splits.
 * @returns The state along with state-mutating actions.
 */
const useSplits = (): ISplits => {
  // Grab the context if it is defined.
  // If not defined, raise an error because the rest of this hook will not work.
  const context = useContext(SplitsContext);
  if (!context) {
    throw new Error("Splits context must be used within a splits component.");
  }

  // We grab the original state from the context.
  const { direction, getSize, setSize, unsetSize } = context;

  // We return this modified context.
  return {
    direction,
    actions: {
      get: getSize,
      set: setSize,
      unset: unsetSize,
    },
  };
};

export default SplitsContext;
export { useSplits };
export type { ISplitsContext, ISplitsActions, ISplits };
