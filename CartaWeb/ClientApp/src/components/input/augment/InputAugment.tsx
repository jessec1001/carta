import { FunctionComponent, HTMLProps } from "react";
import classNames from "classnames";
import { Modify } from "types";

import "./input-augment.css";

/** The props used for the {@link InputAugment} component. */
interface InputAugmentProps {
  /** Whether the augment is supposed to be interactive. */
  interactive?: boolean;
}

/** A component that represents an input augment that overlaps partially with the input field. */
const InputAugment: FunctionComponent<
  Modify<HTMLProps<HTMLDivElement>, InputAugmentProps>
> = ({ interactive, className, children, ...props }) => {
  return (
    <div
      className={classNames("input-augment", className, { interactive })}
      {...props}
    >
      {children}
    </div>
  );
};

export default InputAugment;
export type { InputAugmentProps };
