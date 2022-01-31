import { FC } from "react";

/** The props used for the {@link Node} component. */
interface NodeProps {
  /** A unique identifier to refer to this arrow node by. */
  id: string | number;

  /** The element that this arrow node should use for calculating positions. */
  element?: HTMLElement;
}

/** A component that controls  */
const Node: FC<NodeProps> = ({ id }) => {
  // This component does not actually render anything.
  return null;
};

export default Node;
export type { NodeProps };
