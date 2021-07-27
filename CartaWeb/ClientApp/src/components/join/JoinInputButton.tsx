import { FunctionComponent, HTMLProps } from "react";
import classNames from "classnames";

import "./join.css";

/** A component that specifies a button for an input to be joined to the input itself. */
const JoinInputButton: FunctionComponent<HTMLProps<HTMLButtonElement>> = ({
  type,
  className,
  children,
  ...props
}) => {
  return (
    <button
      type={(type as any) ?? "button"}
      className={classNames("join-input", "join-input-button", className)}
      {...props}
    >
      {children}
    </button>
  );
};

export default JoinInputButton;
