import { FunctionComponent, HTMLProps } from "react";

import "./buttons.css";

/** A button that represents removing or deleting an object. */
const IconRemoveButton: FunctionComponent<HTMLProps<HTMLButtonElement>> = ({
  children,
  type,
  ...props
}) => {
  return (
    <button className="icon-button" type={type as any} {...props}>
      ×
    </button>
  );
};

export default IconRemoveButton;
