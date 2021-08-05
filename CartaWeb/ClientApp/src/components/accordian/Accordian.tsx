import React, { FunctionComponent, PropsWithChildren } from "react";
import { useControllableState } from "hooks";
import AccordianContent, { AccordianContentProps } from "./AccordianContent";
import AccordianHeader, { AccordianHeaderProps } from "./AccordianHeader";

import "./accordian.css";

/** The props used for the {@link Accordian} component. */
interface AccordianProps {
  /** Whether the accordian is toggled. If not specified, the component is uncontrolled. */
  toggled?: boolean;
  /** The event handler that is called whenever the toggle state of the accordian changes. */
  onToggle?: (toggled: boolean) => void;
}

/**
 * A component that renders a toggleable accordian element. The header of this element is always visible while the
 * content of the element is visible only if the accordian is toggled. The toggle element should always be contained in
 * the header of the accordian and will toggle the accordian's state.
 *
 * This component has an optionally controlled toggle state that controls whether the accordian content is visible.
 * @example
 * ```jsx
 * <AccordianContainer>
 *  <AccordianHeader>
 *   My Accordian <!-- This text is always visible. -->
 *   <AccordianToggle caret />
 *  </AccordianHeader>
 *  <AccordianContent>
 *   My accordian has some content. <!-- This text is only visible when the accordian is toggled. -->
 *  </AccordianContent>
 * </AccordianContainer>
 * ```
 */
const Accordian: FunctionComponent<AccordianProps> = ({
  toggled,
  onToggle,
  children,
}) => {
  // We use an optionally controlled toggled state.
  const [actualToggled, setToggled] = useControllableState(
    true,
    toggled,
    onToggle
  );

  // We separate out the children elements into header elements and content elements.
  // We use this partioning to render the children specially.
  const childArray = React.Children.toArray(children);
  const headerChildArray = childArray.filter((child) => {
    return (
      React.isValidElement<AccordianHeaderProps>(child) &&
      child.type === AccordianHeader
    );
  }) as React.ReactElement<PropsWithChildren<AccordianHeaderProps>>[];
  const contentChildArray = childArray.filter((child) => {
    return (
      React.isValidElement<AccordianContentProps>(child) &&
      child.type === AccordianContent
    );
  }) as React.ReactElement<PropsWithChildren<AccordianContentProps>>[];

  return (
    <div className="accordian">
      {/* Render the header elements normally. */}
      {headerChildArray.map((headerChild) => {
        // We create a modified version of the on toggle component with a new on toggle method that changes this state.
        const { onToggle } = headerChild.props;
        const handleToggle = () => {
          if (onToggle) onToggle();
          setToggled((toggled: boolean) => !toggled);
        };

        return React.cloneElement(headerChild, {
          toggled: actualToggled,
          onToggle: handleToggle,
        });
      })}

      {/* Render the content elements with the toggled state. */}
      {contentChildArray.map((contentChild) =>
        React.cloneElement(contentChild, { toggled: actualToggled })
      )}
    </div>
  );
};

export default Accordian;
