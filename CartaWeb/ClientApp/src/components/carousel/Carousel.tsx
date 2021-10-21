import { FunctionComponent, useRef } from "react";
import { CaretIcon } from "components/icons";
import Item from "./Item";

import "./Carousel.css";
import classNames from "classnames";

/** The props used for the {@link Carousel} component. */
interface CarouselProps {
  /**
   * The sizing model of the carousel.
   * - If inner, the arrows of the carousel will remain within the parent container.
   * - If outer, the arrows of the carousel will be outside the parent container and the items in the carousel will
   *   align with the parent bounds.
   */
  sizing?: "inner" | "outer";
}

/** Defines the composition of the compound {@link Carousel} component. */
interface CarouselComposition {
  Item: FunctionComponent;
}

/** A component that displays items in a carousel with a particular number of items at a particular offset. */
const Carousel: FunctionComponent<CarouselProps> & CarouselComposition = ({
  sizing = "outer",
  children,
  ...props
}) => {
  // We use this for finding the correct carousel items to scroll to.
  const entryBoxRef = useRef<HTMLOListElement>(null);

  // These event handlers allow for the carousel to be moved across multiple items.
  const handleMovePrevious = () => {
    if (entryBoxRef.current) {
      // We determine the items in the carousel.
      const element = entryBoxRef.current;
      const subelements = Array.from(
        element.getElementsByClassName("Carousel-Item")
      ) as HTMLDivElement[];

      // We find where we are currently scrolled to.
      const index = subelements.findIndex(
        (subelement) => subelement.offsetLeft > element!.scrollLeft
      );

      // Scroll to the previous element if possible.
      if (index > 0) {
        subelements[index - 1].scrollIntoView({
          behavior: "smooth",
        });
      }
    }
  };
  const handleMoveNext = () => {
    if (entryBoxRef.current) {
      // We determine the items in the carousel.
      const element = entryBoxRef.current;
      const subelements = Array.from(
        element.getElementsByClassName("Carousel-Item")
      ) as HTMLDivElement[];

      // We find where we are currently scrolled to.
      const index = subelements.findIndex(
        (subelement) =>
          subelement.offsetLeft > element!.scrollLeft + element.clientWidth
      );

      // Scroll to the previous element if possible.
      if (index > 0) {
        subelements[index].scrollIntoView({
          behavior: "smooth",
        });
      }
    }
  };

  return (
    <div className={classNames("Carousel", sizing)}>
      {/* Display left arrow. */}
      <span className="Carousel-Arrow" onClick={handleMovePrevious}>
        <CaretIcon direction="left" />
      </span>

      {/* Display items. */}
      <ol ref={entryBoxRef} className="Carousel-Entries" role="presentation">
        {children}
      </ol>

      {/* Display right arrow. */}
      <span className="Carousel-Arrow" onClick={handleMoveNext}>
        <CaretIcon direction="right" />
      </span>
    </div>
  );
};
Carousel.Item = Item;

export default Carousel;
export type { CarouselProps };
