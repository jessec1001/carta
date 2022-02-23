import { createContext, useCallback, useContext } from "react";

/** Represents the type of an arrow node identifier. */
type ArrowsId = string | number;

/**
 * The type of value used for the {@link ArrowsContext}.
 * This is meant to provide the minimal functionality to use arrows.
 */
interface IArrowsContext {
  /** Sets the specified arrows node. */
  setNode: <T extends HTMLElement>(id: ArrowsId, element: T | null) => void;
  /** Gets the specified arrows node. */
  getNode: <T extends HTMLElement>(id: ArrowsId) => T | undefined;
  /** Remove the specified arrows node. */
  removeNode: (id: ArrowsId) => void;
  /** Fetches the position of the specified arrows node. */
  positionNode: (id: ArrowsId) => [x: number, y: number] | undefined;
}
/**
 * Exposes the state of, and the actions performable on an {@link Arrows}.
 */
interface IArrows {
  /** Accesses the methods applicable to a specific node. */
  nodes: (id: ArrowsId) => {
    /** Sets the node. */
    set: <T extends HTMLElement>(element: T | null) => void;
    /** Gets the node. */
    get: <T extends HTMLElement>() => T | undefined;
    /** Removes the node. */
    remove: () => void;
    /** Fetches the node position. */
    position: () => [x: number, y: number] | undefined;
  };
}

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

  // Grab the original functions from the context.
  const { setNode, getNode, removeNode, positionNode } = context;

  // Construct the nodes helper function.
  const nodes = useCallback(
    (id: ArrowsId) => {
      return {
        set: <T extends HTMLElement>(element: T | null) =>
          setNode<T>(id, element),
        get: <T extends HTMLElement>() => getNode<T>(id),
        remove: () => removeNode(id),
        position: () => positionNode(id),
      };
    },
    [getNode, positionNode, removeNode, setNode]
  );

  // Return a modified context that is more applicable.
  return { nodes };
};

export default ArrowsContext;
export { useArrows };
export type { ArrowsId, IArrowsContext, IArrows };
