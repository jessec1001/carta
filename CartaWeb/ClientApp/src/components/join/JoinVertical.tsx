import { FunctionComponent } from "react";
import classNames from "classnames";

import "./join.css";

/** The props used for the {@link JoinVertical} component. */
interface JoinVerticalProps {
  /** The directions in which the element is joined to. */
  directions: ("top" | "bottom")[];
  /** Whether this element should grow to fit the container. */
  grow?: boolean;
}

/** A component that joins the children components vertically in a specified direction. */
const JoinVertical: FunctionComponent<JoinVerticalProps> = ({
  grow,
  directions,
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

export default JoinVertical;
