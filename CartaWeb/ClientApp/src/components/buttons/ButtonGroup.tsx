import { FunctionComponent } from "react";

import "./buttons.css";

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
  if (connected)
    return <div className="button-group connected">{children}</div>;
  else return <div className="button-group">{children}</div>;
};

export default ButtonGroup;
export type { ButtonGroup };
