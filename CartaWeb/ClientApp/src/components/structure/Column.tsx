import classNames from "classnames";
import { FunctionComponent, HTMLProps } from "react";

import "./structure.css";

/** A component that represents a column within a row. */
const Column: FunctionComponent<HTMLProps<HTMLDivElement>> = ({
  className,
  children,
  ...props
}) => {
  return (
    <div className={classNames("column", className)} {...props}>
      {children}
    </div>
  );
};

export default Column;
