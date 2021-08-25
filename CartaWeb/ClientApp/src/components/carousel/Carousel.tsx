import React, { FunctionComponent, useEffect, useLayoutEffect } from "react";
import { useControllableState } from "hooks";
import { BlockButton } from "components/buttons";
import { CaretIcon } from "components/icons";
import Item from "./Item";

/** The props used for the {@link Carousel} component. */
interface CarouselProps {
  /**
   * The sizing of the carousel within its parent.
   * - If `"inner"`, represents that the entire carousel fits within its parent width.
   * - If `"outer"`, represents that the arrows for the carousel are fit outside its parent width.
   */
  sizing: "inner" | "outer";

  /** The number of items to display at a time. Optional; if not specified, will use some default standard sizes. */
  itemCount?: number;
  /**
   * The offset to the start of the currently displayed items. Optional; if not specified, this component will be
   * uncontrolled.
   */
  itemOffset?: number;

  /** The event handler that is called whenever the item offset is changed. */
  onItemOffsetChanged?: (itemOffset: number) => void;
}

/** Defines the composition of the compound {@link Carousel} component. */
interface CarouselComposition {
  Item: FunctionComponent;
}

/** A component that displays items in a carousel with a particular number of items at a particular offset. */
const Carousel: FunctionComponent<CarouselProps> & CarouselComposition = ({
  sizing = "outer",
  itemCount,
  itemOffset,
  onItemOffsetChanged,
  children,
  ...props
}) => {
  // We allows the item offset and count to be optionally controllable to give users of this component more control.
  const [actualItemOffset, setItemOffset] = useControllableState<number>(
    0,
    itemOffset,
    onItemOffsetChanged
  );
  const [actualItemCount, setItemCount] = useControllableState<number>(
    1,
    itemCount
  );

  // We use the child count to clamp the range of the item offset.
  const actualChildCount = React.Children.count(children);
  useEffect(() => {
    setItemOffset((itemOffset) => {
      return Math.max(
        0,
        Math.min(actualChildCount - actualItemCount, itemOffset)
      );
    });
  }, [setItemOffset, actualChildCount, actualItemCount]);

  // TODO: Figure out how to implement.
  // We use a layout effect to calculate the maximum height of this element when all children are rendered.
  // We calculate this by psuedo-rendering all elements in a single contiguous horizontal strip.
  useLayoutEffect(() => {}, []);

  // These event handlers allow for the carousel to be moved across multiple items.
  const handleMovePrevious = () => {
    setItemOffset((itemOffset) =>
      Math.min(actualChildCount - actualItemCount, itemOffset + 1)
    );
  };
  const handleMoveNext = () => {
    setItemOffset((itemOffset) => Math.max(0, itemOffset - 1));
  };

  // TODO: We should try to cleverly render each of the items but hide the items that should be hidden in such a way
  // that the maximum height of the carousel is maintained.
  return (
    <div>
      <BlockButton onClick={handleMovePrevious}>
        <CaretIcon direction="left" />
      </BlockButton>
      <ol role="presentation">{children}</ol>
      <BlockButton onClick={handleMoveNext}>
        <CaretIcon direction="right" />
      </BlockButton>
    </div>
  );
};
Carousel.Item = Item;

export default Carousel;
export type { CarouselProps };
