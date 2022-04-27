import React, { FunctionComponent } from "react";
import styles from "./Dropdown.module.css";

/** A component that displays the collapsible area of a dropdown menu. */
const Area: FunctionComponent = ({ children }) => {
  return (
    <div className={styles.dropdownArea}>
      <ul role="navigation" className={styles.dropdownArea}>
        {React.Children.map(children, (child) => (
          <li>{child}</li>
        ))}
      </ul>
    </div>
  );
};

export default Area;
