import { FunctionComponent } from "react";
import classNames from "classnames";

/** The props used for the {@link AccordianContent} component. */
interface AccordianContentProps {
  /** Whether the content should be toggled (visible). */
  toggled?: boolean;
}

/** A component that contains the content for an {@link Accordian} component. */
const AccordianContent: FunctionComponent<AccordianContentProps> = ({
  toggled,
  children,
}) => {
  // We return a component that should be rendered conditionally based on the toggled state.
  return (
    <div className={classNames("accordian-content", { toggled })}>
      {children}
    </div>
  );
};

export default AccordianContent;
export type { AccordianContentProps };
