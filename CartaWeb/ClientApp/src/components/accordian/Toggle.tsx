import { FunctionComponent, HTMLAttributes } from "react";
import classNames from "classnames";
import { CaretIcon } from "components/icons";
import { useAccordian } from "./Context";
import styles from "./Accordian.module.css";

/** The props used for the {@link Toggle} component. */
interface ToggleProps extends HTMLAttributes<HTMLButtonElement> {
  /** Whether or not the toggler should include a standard caret icon. */
  caret?: boolean;
}

/** A component that toggles an accordian component and can optionally display as a caret icon. */
const Toggle: FunctionComponent<ToggleProps> = ({
  caret,
  children,
  className,
  onClick = () => {},
  ...props
}) => {
  // Get the state of the accordian and the actions to operate on it.
  const { toggled, actions } = useAccordian();

  return (
    <button
      className={classNames(styles.toggle, className)}
      onClick={(e) => {
        onClick(e);
        actions.toggle();
      }}
      {...props}
    >
      {/* Render the children of this component normally. */}
      {children}

      {/* If the caret flag has been set, we use a caret icon to  */}
      {caret && <CaretIcon direction={toggled ? "down" : "right"} />}
    </button>
  );
};

export default Toggle;
export type { ToggleProps };
