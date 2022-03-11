import { ComponentProps, FC, useCallback, useEffect, useRef } from "react";
import { useArrows } from "./Context";

/** The props used for the {@link Node} component. */
interface NodeProps extends Omit<ComponentProps<"div">, "id"> {
  /** A unique identifier to refer to this arrow node by. */
  id: string | number;

  /** If specified, indicates the interval, in milliseconds, that we should poll the position of the node. */
  pollingInterval?: number;
}

/** A component that controls  */
const Node: FC<NodeProps> = ({ id, pollingInterval, children, ...props }) => {
  // We use the arrows context to store the nodes and their bounding boxes.
  const element = useRef<HTMLDivElement>(null);
  const { nodes } = useArrows();

  const updateBounds = useCallback(() => {
    if (!element.current) return;

    // Get the bounding box of the node.
    const boundsNext = element.current.getBoundingClientRect();
    const boundsPrev = nodes(id).get();

    // Update the bounding boxes if necessary.
    if (boundsNext === boundsPrev) return;
    if (
      !boundsNext ||
      !boundsPrev ||
      boundsNext.left !== boundsPrev.left ||
      boundsNext.right !== boundsPrev.right ||
      boundsNext.top !== boundsPrev.top ||
      boundsNext.bottom !== boundsPrev.bottom
    ) {
      nodes(id).set(boundsNext);
    }
  }, [id, nodes]);

  // We update the bounding boxes when the node is mounted.
  useEffect(() => {
    updateBounds();
  }, [updateBounds]);

  // We update the bounding boxes when polling.
  useEffect(() => {
    if (!pollingInterval) return;

    const interval = setInterval(updateBounds, pollingInterval);
    return () => clearInterval(interval);
  }, [pollingInterval, updateBounds]);

  // This component does not actually render anything.
  return (
    <div ref={element} {...props}>
      {children}
    </div>
  );
};

export default Node;
export type { NodeProps };
