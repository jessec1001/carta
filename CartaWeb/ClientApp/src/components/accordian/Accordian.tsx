import { FunctionComponent, HTMLAttributes } from "react";
import { useControllableState } from "hooks";
import Content from "./Content";
import Header from "./Header";
import Toggle from "./Toggle";
import AccordianContext from "./Context";

/** The props used for the {@link Accordian} component. */
interface AccordianProps extends HTMLAttributes<HTMLDivElement> {
  /** Whether the accordian is toggled. If not specified, the component is uncontrolled. */
  toggled?: boolean;
  /** Whether the accordian is toggled initially. Used for uncontrolled components and their initial state. */
  initialToggled?: boolean;
  /** The event handler that is called whenever the toggle state of the accordian changes. */
  onToggle?: (toggled: boolean) => void;
}

/**
 * Defines the composition of the compound {@link Accordian} component.
 * @borrows Header as Header
 * @borrows Content as Content
 * @borrows Toggle as Toggle
 */
interface AccordianComposition {
  Header: typeof Header;
  Content: typeof Content;
  Toggle: typeof Toggle;
}

/**
 * A component that renders a toggleable accordian element. The header of this element is always visible while the
 * content of the element is visible only if the accordian is toggled. The toggle element should always be contained in
 * the header of the accordian and will toggle the accordian's state.
 *
 * This component has an optionally controlled toggle state that controls whether the accordian content is visible.
 * @example
 * ```jsx
 * <Accordian>
 *  <Accordian.Header>
 *   My Accordian <!-- This text is always visible. -->
 *   <Accordian.Toggle caret />
 *  </Accordian.Header>
 *  <Accordian.Content>
 *   My accordian has some content. <!-- This text is only visible when the accordian is toggled. -->
 *  </Accordian.Content>
 * </Accordian>
 * ```
 */
const Accordian: FunctionComponent<AccordianProps> & AccordianComposition = ({
  toggled,
  initialToggled = true,
  onToggle,
  children,
  ...props
}) => {
  // We use an optionally controlled toggled state.
  const [actualToggled, setToggled] = useControllableState(
    initialToggled,
    toggled,
    onToggle
  );

  // We wrap the child elements in an accordian context to provide the accordian functionality.
  return (
    <AccordianContext.Provider
      value={{ toggled: actualToggled, setToggled: setToggled }}
    >
      <div {...props}>{children}</div>
    </AccordianContext.Provider>
  );
};
Accordian.Content = Content;
Accordian.Header = Header;
Accordian.Toggle = Toggle;

export default Accordian;
export type { AccordianProps };
