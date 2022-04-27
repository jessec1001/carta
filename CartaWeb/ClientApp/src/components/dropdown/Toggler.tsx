import { FunctionComponent } from "react";
import { CaretIcon } from "components/icons";
import styles from "./Dropdown.module.css";
import { useDropdown } from "./Context";

/** The props used for the {@link Toggler} component. */
interface DropdownTogglerProps {
  /** Whether to include a caret icon or not. */
  caret?: boolean;
}

/** A component that is used to toggle a dropdown menu that can have an optionally added caret icon. */
const Toggler: FunctionComponent<DropdownTogglerProps> = ({
  caret,
  children,
}) => {
  const { actions } = useDropdown();

  return (
    <div className={styles.dropdownToggler} onClick={() => actions.toggle()}>
      <span className="normal-text">
        {children}
        {caret && <CaretIcon padded />}
      </span>
    </div>
  );
};

export default Toggler;
export type { DropdownTogglerProps };
