import { FC, useCallback, useState } from "react";
import Arrow from "./Arrow";
import ArrowsContext, { ArrowsId } from "./Context";

/** The props used for the {@link Arrows} component. */
interface ArrowsProps {}

/**
 * Defines the composition of the compound {@link Arrows} component.
 * @borrows Arrow as Arrow
 */
interface ArrowsComposition {
  Arrow: typeof Arrow;
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

const Arrows: FC<ArrowsProps> & ArrowsComposition = ({ children }) => {
  // We store the arrow nodes and their bounding boxes in this state.
  const [nodes, setNodes] = useState<Map<ArrowsId, HTMLElement>>(new Map());
  const [bounds, setBounds] = useState<Map<ArrowsId, DOMRect>>(new Map());

  // Define all of the functions required to interact with the arrows component.
  const removeNode = useCallback((id: ArrowsId) => {
    // Remove the node and its bounding box from the state.
    setNodes((nodes) => {
      nodes = new Map(nodes);
      nodes.delete(id);
      return nodes;
    });
    setBounds((bounds) => {
      bounds = new Map(bounds);
      bounds.delete(id);
      return bounds;
    });
  }, []);
  const setNode = useCallback(
    <T extends HTMLElement>(id: ArrowsId, node: T | null) => {
      if (node === null) removeNode(id);
      else {
        setNodes((nodes) => {
          nodes = new Map(nodes);
          nodes.set(id, node);
          return nodes;
        });
        setBounds((bounds) => {
          bounds = new Map(bounds);
          bounds.set(id, node.getBoundingClientRect());
          return bounds;
        });
      }
    },
    [removeNode]
  );
  const getNode = useCallback(
    <T extends HTMLElement>(id: ArrowsId) => {
      // Get the node from the state.
      return nodes.get(id) as T | undefined;
    },
    [nodes]
  );
  const positionNode = useCallback(
    (id: ArrowsId) => {
      // Get the bounding rectangle of the specified node.
      const boundingRect = bounds.get(id);
      if (!boundingRect) return undefined;

      // TODO: Use an anchor to specify the relative position on the bounding box.
      // Return the center of the bounding rectangle.
      return [
        boundingRect.left + boundingRect.width / 2,
        boundingRect.top + boundingRect.height / 2,
      ] as [number, number];
    },
    [bounds]
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

export default Arrows;
export type { ArrowsProps };
