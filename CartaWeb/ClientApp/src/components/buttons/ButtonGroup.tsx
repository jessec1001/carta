import classNames from "classnames";
import { FunctionComponent } from "react";

import "./ButtonGroup.css";

/** The props used for the {@link ButtonGroup} component. */
interface ButtonGroupProps {
  /** Whether the buttons in the group should be connected together. */
  connected?: boolean;
}

/** A component that renders a collection of buttons as a group. */
const ButtonGroup: FunctionComponent<ButtonGroupProps> = ({
  connected,
  children,
}) => {
  return (
    <div className={classNames("ButtonGroup", { connected })}>{children}</div>
  );
};

export default ButtonGroup;
export type { ButtonGroup };
