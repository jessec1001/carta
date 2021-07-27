import React, { FunctionComponent } from "react";
import classNames from "classnames";
import JoinHorizontal from "./JoinHorizontal";
import JoinVertical from "./JoinVertical";

import "./join.css";

/** The props used for the {@link JoinContainer} component. */
interface JoinContainerProps {
  /** The direction of the joined elements within the container. If not specified, elements are not joined. */
  direction?: "horizontal" | "vertical";
  /**
   * A special grow parameter for common join layouts. Only works if direction is specified.
   *
   * - `grow-first` makes the first rendered child element grow.
   * - `grow-last` makes the last rendered child element grow.
   */
  grow?: "grow-first" | "grow-last";
}

/** A component that automatically joins child components together based on the specified direction. */
const JoinContainer: FunctionComponent<JoinContainerProps> = ({
  grow,
  direction,
  children,
}) => {
  // We find all of the truthy children elements to render.
  const childArray = React.Children.toArray(children).filter((x) => !!x);
  const childCount = childArray.length;

  return (
    <div className={classNames("join-container", direction)}>
      {childArray.map((child, index) => {
        // Check for first or last grow parameter .
        let subgrow: boolean = false;
        if (index === 0 && grow === "grow-first") subgrow = true;
        if (index + 1 === childCount && grow === "grow-last") subgrow = true;

        // Generate new children based on the directionality of the component.
        switch (direction) {
          case "horizontal":
            // Horizontal joins connect left-right.
            let directionsHorizontal: ("left" | "right")[] = [];
            if (index > 0) directionsHorizontal.push("left");
            if (index + 1 < childCount) directionsHorizontal.push("right");
            return (
              <JoinHorizontal directions={directionsHorizontal} grow={subgrow}>
                {child}
              </JoinHorizontal>
            );
          case "vertical":
            // Vertical joins connect top-bottom.
            let directionsVertical: ("top" | "bottom")[] = [];
            if (index > 0) directionsVertical.push("top");
            if (index + 1 < childCount) directionsVertical.push("bottom");
            return (
              <JoinVertical directions={directionsVertical} grow={subgrow}>
                {child}
              </JoinVertical>
            );
          default:
            return child;
        }
      })}
    </div>
  );
};

export default JoinContainer;
export type { JoinContainerProps };
