import { FunctionComponent } from "react";

import "./input.css";

/** The props used for the {@link OptionInput} component. */
interface OptionInputProps {
  /** The value that this option component takes on. */
  value?: any;
  /** The textual alias showed in some components such as comboboxes. */
  alias?: string;
  /** Whether this option input is actually selectable. */
  unselectable?: boolean;
  /** The event handler for when an option is selected. */
  onClick?: (value: any) => void;
}

/**
 * A component that represents an option within an {@link OptionSelectorInput}.
 * Useful in components such as dropdowns and comboboxes.
 */
const OptionInput: FunctionComponent<OptionInputProps> = ({
  value,
  onClick,
  children,
}) => {
  // We handle a click by passing the value upwards.
  const handleClick = () => {
    if (onClick) onClick(value);
  };

  // We assume that this option input will always be contained in a list.
  return <span onClick={handleClick}>{children}</span>;
};

export default OptionInput;
export type { OptionInputProps };
