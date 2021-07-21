import { FunctionComponent, HTMLProps } from "react";

import "./buttons.css";

/** A button that represents adding or creating an object. */
const IconAddButton: FunctionComponent<HTMLProps<HTMLButtonElement>> = ({
  children,
  type,
  ...props
}) => {
  return (
    <button className="icon-button" type={type as any} {...props}>
      +
    </button>
  );
};

export default IconAddButton;
