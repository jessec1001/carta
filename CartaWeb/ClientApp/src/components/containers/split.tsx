import React, {
  forwardRef,
  FunctionComponent,
  PropsWithChildren,
  useEffect,
  useRef,
  useState,
} from "react";
import { useControllableState } from "hooks";
import classNames from "classnames";

import "./split.css";

// TODO: Refactor all of this to share a similar definition as the tabs components.

// TODO: Make all split panels have a menu bar that can contain the tab bar and additional buttons for manipulation of
//       the view hierarchy.

/** A component that renders a handle to adjust the size of a split pane. */
const SplitGutter = forwardRef<HTMLDivElement>((_, ref) => {
  return <div ref={ref} className="split-gutter" />;
});

/** The props used for the {@link SplitUncollapsedPane} component. */
interface SplitUncollapsedPaneProps {
  /** The ratio of size that the pane shares in the container. */
  size: number;
}
/** A component that renders an uncollapsed (non-zero size) pane of a split area. */
const SplitUncollapsedPane = forwardRef<
  HTMLDivElement,
  PropsWithChildren<SplitUncollapsedPaneProps>
>(({ size, children }, ref) => {
  // TODO: An uncollapsed pane should have a button/control that allows for collapsing the pane.
  return (
    <div
      ref={ref}
      className="split-pane uncollapsed"
      style={{
        flex: size,
      }}
    >
      {children}
    </div>
  );
});

/** The props used for the {@link SplitCollapsedPaneProps} component. */
interface SplitCollapsedPaneProps {
  collapsePoint: number;
}
/** A component that renders a collapsed (zero size) pane of a split area. */
const SplitCollapsedPane = forwardRef<HTMLDivElement, SplitCollapsedPaneProps>(
  ({ collapsePoint }, ref) => {
    // TODO: A collapsed pane should have a button/control that allows for uncollapsing the pane.
    // TODO: Add a button that can be dragged to move the collapsed pane and its surrounding gutters.
    return (
      <div
        ref={ref}
        className="split-pane collapsed"
        style={{
          ["--collapse-point" as any]: `${collapsePoint}px`,
        }}
      >
        {/* TODO: Add in support to expand collapsed split panes. */}
        {/*
        <span className="normal-text split-pane-button vertical">
          <PushIcon direction={"down"} />
        </span>
        <span className="normal-text split-pane-button vertical">
          <PushIcon direction={"up"} />
        </span>
        <span className="normal-text split-pane-button horizontal">
          <PushIcon direction={"right"} />
        </span>
        <span className="normal-text split-pane-button horizontal">
          <PushIcon direction={"left"} />
        </span>
        */}
      </div>
    );
  }
);

/** The props used for the {@link SplitArea} component. */
interface SplitAreaProps {
  /** The direction that the split area should flow. Note that this is the opposite direction of the gutter span. */
  direction: "vertical" | "horizontal";

  /** Whether individial panes within the split area should be collapsible. */
  collapseable?: boolean;
  /** The threshold (in pixels) at which a pane should become collapsed. */
  collapsePoint?: number;

  /**
   * The sizes of the split panes inside of the split area.
   * A value equal to zero indicates that the pane is collapsed.
   * To differentiate between collapsed to the start and collapsed to the end of the area,
   * we use -0 to indicate the start and +0 to indicate the end.
   */
  sizes?: number[];
  /** The event handler that is called when the sizes of the split panes are changed. */
  onSizesChanged?: (sizes: number[]) => void;
}
/** A component that renders children as split panes within it. */
const SplitArea: FunctionComponent<SplitAreaProps> = ({
  direction,
  collapseable = true,
  collapsePoint = 24,
  sizes,
  onSizesChanged,
  children,
}) => {
  // If sizes were specified and the number of sizes does not match the number of children, we throw an error.
  const childCount = React.Children.count(children);
  if (sizes && sizes.length !== childCount)
    throw new Error("Sizes array must have same length as number of children.");

  // The sizes are an optionally controlled prop the define the size of child elements.
  // Dynamic layouts will tend to control the sizes whereas static layouts will tend to leave the sizes uncontrolled.
  const [actualSizes, setSizes] = useControllableState(
    () => {
      const initialSizes = [];
      for (let k = 0; k < childCount; k++) initialSizes.push(1.0 / childCount);
      return initialSizes;
    },
    sizes,
    onSizesChanged
  );

  // TODO: Add some gutters to the start and end that allow for creating new elements
  // to the start and end respectively.

  // TODO: Handle children changing.
  // We can perform a clever computation to check when the children are changed over an iteration.
  // We can use the inserted or deleted children to determine how to adjust the pane sizes.
  useEffect(() => {
    if (actualSizes.length < childCount) {
      setSizes((sizes) => {
        const newSizes = [];
        for (let k = 0; k < childCount; k++) {
          if (k < sizes.length)
            newSizes[k] = sizes[k] * (actualSizes.length / childCount);
          else newSizes[k] = 1.0 / childCount;
        }
        return newSizes;
      });
    } else if (actualSizes.length > childCount) {
    }
  }, [setSizes, actualSizes, childCount]);

  // We use references to the gutters to determine split pane sizes when the gutters are moved.
  // We also use references to the children elements to grab their sizes in relation to their parent.
  // Note that for N children, there are N - 1 gutters dividing them.
  const gutterRefs = useRef<(HTMLDivElement | null)[]>(
    Array<null>(Math.max(0, childCount - 1)).fill(null)
  );
  const paneRefs = useRef<(HTMLDivElement | null)[]>(
    Array<null>(childCount).fill(null)
  );

  // This state represents the index of the gutter (from 0 to N - 1) currently being dragged.
  // Set to null when no gutter is being dragged.
  const [draggedGutter, setDraggedGutter] = useState<number | null>(null);

  // Set up some event handlers for when the mouse state changes.
  // When the mouse is pressed or released on a gutter, we start or end dragging that gutter respectively.
  const handleMouseDown = (event: React.MouseEvent) => {
    gutterRefs.current.forEach((gutterRef, gutterIndex) => {
      if (gutterRef?.isSameNode(event.target as Node)) {
        setDraggedGutter(gutterIndex);

        // We prevent default mouse event which may select text or perform some other undesireable action.
        event.preventDefault();
      }
    });
  };
  const handleMouseUp = (event: React.MouseEvent) => {
    setDraggedGutter(null);
  };
  const handleMouseEnter = (event: React.MouseEvent) => {};
  const handleMouseLeave = (event: React.MouseEvent) => {
    setDraggedGutter(null);
  };
  const handleMouseMove = (event: React.MouseEvent) => {
    if (draggedGutter !== null) {
      // Compute the size of the container and of the size of each pane in the major direction.
      let totalSize: number = 0;
      let paneSizes: number[] = [];
      for (let k = 0; k < childCount; k++) {
        let paneSize = 0;

        // We compute the size of each pane in the split area.
        // If the area of a pane is zero, we do not count it in this calculation.
        const rect = paneRefs.current[k]?.getBoundingClientRect();
        if (rect === undefined || (collapseable && actualSizes[k] === 0)) {
          paneSize = 0;
        } else {
          paneSize = direction === "horizontal" ? rect.width : rect.height;
        }

        // We collect the sizes as a summary.
        totalSize += paneSize;
        paneSizes.push(paneSize);
      }

      // We grab a copy of the current sizes to modify as we compute the distribution.
      const newSizes = [...actualSizes];

      // TODO: Consider the case where panes are collapsed with zero size.
      // TODO: Allow for pushing subsequent panes.
      // If we are dragging gutter number k, pane k and pane k + 1 should be adjusted.
      const rect = paneRefs.current[draggedGutter]?.getBoundingClientRect();
      if (rect !== undefined) {
        // The difference in size is measured arbitrarily on the first of affected panes.
        // The difference is measured by comparing current size to new size accounting for mouse position.
        // If the difference falls below the collapse point, force the pane to collapse completely.
        let paneSizeDifference =
          direction === "horizontal"
            ? event.clientX - rect.right
            : event.clientY - rect.bottom;

        // Notice that either or none of these new sizes can be beyond the collapse point.
        // At or beyond the collapse point, we will reduce the size to zero.
        if (collapseable) {
          if (
            paneSizes[draggedGutter + 0] + paneSizeDifference <=
            collapsePoint
          )
            paneSizeDifference = -paneSizes[draggedGutter + 0];
          if (
            paneSizes[draggedGutter + 1] - paneSizeDifference <=
            collapsePoint
          )
            paneSizeDifference = paneSizes[draggedGutter + 1];
        }

        // Adjust the size of the panes connected by the dragged gutter.
        newSizes[draggedGutter] =
          (paneSizes[draggedGutter] + paneSizeDifference) / totalSize;
        newSizes[draggedGutter + 1] =
          (paneSizes[draggedGutter + 1] - paneSizeDifference) / totalSize;
        setSizes(newSizes);
      }
    }
  };

  return (
    <div
      className={classNames("split-area", direction)}
      onMouseDown={handleMouseDown}
      onMouseUp={handleMouseUp}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
      onMouseMove={handleMouseMove}
    >
      {React.Children.map(children, (child, index) => {
        return (
          <React.Fragment key={index}>
            {/* We render a gutter between elements and store a reference to reference it for movement later. */}
            {index > 0 && (
              <SplitGutter
                ref={(gutter) => (gutterRefs.current[index - 1] = gutter)}
              />
            )}

            {/* Render the pane itself with the size determined by the component. */}
            {/* If the size is absolutely zero, we render the collapsed version. */}
            {/* TODO: Fix panes being collapsed lose their state because they are no longer rendered. */}
            {actualSizes[index] <= 0 && collapseable ? (
              <SplitCollapsedPane
                collapsePoint={collapsePoint}
                ref={(pane) => (paneRefs.current[index] = pane)}
              />
            ) : (
              <SplitUncollapsedPane
                size={actualSizes[index]}
                ref={(pane) => (paneRefs.current[index] = pane)}
              >
                {child}
              </SplitUncollapsedPane>
            )}
          </React.Fragment>
        );
      })}
    </div>
  );
};

export { SplitArea, SplitGutter, SplitUncollapsedPane, SplitCollapsedPane };
export type { SplitAreaProps };

/** Question #1
 *
 * What do we do when the sizes array does not match the
 *
 */
