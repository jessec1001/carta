import { FC, useCallback, useState } from "react";
import Arrow from "./Arrow";
import Node from "./Node";
import ArrowsContext, { ArrowsId } from "./Context";

/** The props used for the {@link Arrows} component. */
interface ArrowsProps {
  /** The element that should contain all of the arrows. */
  element: HTMLElement | null;
}

/**
 * Defines the composition of the compound {@link Arrows} component.
 * @borrows Arrow as Arrow
 * @borrows Node as Node
 */
interface ArrowsComposition {
  Arrow: typeof Arrow;
  Node: typeof Node;
}

/** A component that describes how particular graphical elements of SVG arrows are rendered. */
const ArrowsDefinitions: FC = () => (
  <svg>
    <defs>
      <marker
        id="Arrows-arrow"
        viewBox="0 0 10 10"
        refX="1"
        refY="5"
        markerWidth="6"
        markerHeight="6"
        orient="auto"
      >
        <path d="M 0 0 L 10 5 L 0 10 z" />
      </marker>
    </defs>
  </svg>
);

const Arrows: FC<ArrowsProps> & ArrowsComposition = ({ element, children }) => {
  // We store the arrow nodes and their bounding boxes in this state.
  const [nodeBounds, setNodeBounds] = useState<Map<ArrowsId, DOMRect>>(
    new Map()
  );

  // Define all of the functions required to interact with the arrows component.
  const removeNode = useCallback((id: ArrowsId) => {
    // Remove the node's bounding box from the state.
    setNodeBounds((bounds) => {
      bounds = new Map(bounds);
      bounds.delete(id);
      return bounds;
    });
  }, []);
  const setNode = useCallback(
    (id: ArrowsId, bounds: DOMRect | null) => {
      if (bounds === null) removeNode(id);
      else {
        setNodeBounds((nodeBounds) => {
          nodeBounds = new Map(nodeBounds);
          nodeBounds.set(id, bounds);
          return nodeBounds;
        });
      }
    },
    [removeNode]
  );
  const getNode = useCallback(
    (id: ArrowsId) => {
      // Get the node from the state.
      return nodeBounds.get(id);
    },
    [nodeBounds]
  );
  const positionNode = useCallback(
    (id: ArrowsId) => {
      // Get the bounding rectangle of the specified node.
      const boundingRect = nodeBounds.get(id);
      if (!boundingRect) return undefined;
      if (!element) return undefined;

      // TODO: Use an anchor to specify the relative position on the bounding box.
      // Get the offset of the arrows element.
      const { left, top } = element.getBoundingClientRect();

      // Return the center of the bounding rectangle.
      return [
        boundingRect.left + boundingRect.width / 2 - left,
        boundingRect.top + boundingRect.height / 2 - top,
      ] as [number, number];
    },
    [element, nodeBounds]
  );

  return (
    <ArrowsContext.Provider
      value={{ setNode, getNode, removeNode, positionNode }}
    >
      <ArrowsDefinitions />
      {children}
    </ArrowsContext.Provider>
  );
};
Arrows.Arrow = Arrow;
Arrows.Node = Node;

export default Arrows;
export type { ArrowsProps };
