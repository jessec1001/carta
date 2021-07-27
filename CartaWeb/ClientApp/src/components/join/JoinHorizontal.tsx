import { FunctionComponent } from "react";
import classNames from "classnames";

import "./join.css";

/** The props used for the {@link JoinHorizontal} component. */
interface JoinHorizontalProps {
  /** The directions in which the element is joined to. */
  directions: ("left" | "right")[];
  /** Whether this element should grow to fit the container. */
  grow?: boolean;
}

/** A component that joins the children components horizontally in a specified direction. */
const JoinHorizontal: FunctionComponent<JoinHorizontalProps> = ({
  directions,
  grow,
  children,
}) => {
  return (
    <div
      className={classNames(
        directions.map((direction) => `join-${direction}`),
        { grow }
      )}
    >
      {children}
    </div>
  );
};

export default JoinHorizontal;
