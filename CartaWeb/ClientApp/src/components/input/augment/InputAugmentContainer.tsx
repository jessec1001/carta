import { FunctionComponent } from "react";
import classNames from "classnames";

import "./input-augment.css";

/** The props used for the {@link InputAugmentContainer} component. */
interface InputAugmentContainerProps {
  /** The side that the input augment is located on. */
  side: "left" | "right";
}

/** A component that contains a input and augments for the input. */
const InputAugmentContainer: FunctionComponent<InputAugmentContainerProps> = ({
  side,
  children,
}) => {
  return (
    <div className={classNames("input-augment-container", side)}>
      {children}
    </div>
  );
};

export default InputAugmentContainer;
export type { InputAugmentContainerProps };
