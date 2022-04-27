import { FunctionComponent, HTMLAttributes } from "react";
import styles from "./Dropdown.module.css";

/** A component that represents an item in a dropdown menu. */
const Item: FunctionComponent<HTMLAttributes<HTMLSpanElement>> = ({
  children,
  ...props
}) => {
  return (
    <span className={styles.dropdownItem} {...props}>
      {children}
    </span>
  );
};

export default Item;
