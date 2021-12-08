import classNames from "classnames";
import { FunctionComponent, HTMLProps } from "react";

import "./structure.css";

/** A component that represents a contiguous row on the screen. */
const Row: FunctionComponent<HTMLProps<HTMLDivElement>> = ({
  className,
  children,
  ...props
}) => {
  return (
    <div className={classNames("row", className)} {...props}>
      {children}
    </div>
  );
};

export default Row;
