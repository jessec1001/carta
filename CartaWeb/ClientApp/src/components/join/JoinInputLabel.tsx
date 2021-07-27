import { FunctionComponent, HTMLProps } from "react";
import classNames from "classnames";

import "./join.css";

/** A component that specifies a label for an input to be joined to the input itself. */
const JoinInputLabel: FunctionComponent<HTMLProps<HTMLDivElement>> = ({
  className,
  children,
  ...props
}) => {
  return (
    <div
      className={classNames("join-input", "join-input-label", className)}
      {...props}
    >
      {children}
    </div>
  );
};

export default JoinInputLabel;
