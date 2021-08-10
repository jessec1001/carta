import { FunctionComponent } from "react";
import { CaretIcon } from "components/icons";

import "./dropdown.css";

/** The props used for the {@link DropdownToggler} component. */
interface DropdownTogglerProps {
  /** Whether to include a caret icon or not. */
  caret?: boolean;
}

/** A component that is used to toggle a dropdown menu that can have an optionally added caret icon. */
const DropdownToggler: FunctionComponent<DropdownTogglerProps> = ({
  caret,
  children,
}) => {
  return (
    <span className="normal-text">
      {children}
      {caret && <CaretIcon padded />}
    </span>
  );
};

export default DropdownToggler;
export type { DropdownTogglerProps };
