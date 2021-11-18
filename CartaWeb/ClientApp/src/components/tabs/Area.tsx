import { FunctionComponent, HTMLAttributes } from "react";
import classNames from "classnames";
import { Modify } from "types";

import "./Area.css";

/** The props used for the {@link Area} component. */
interface AreaProps {
  /** The direction that the tab area should stretch. */
  direction: "horizontal" | "vertical";
  /** Whether the tabs area should be stretched inside of a flexbox. */
  flex?: boolean;
}

/** The area component containing all relevant tab components. */
const Area: FunctionComponent<
  Modify<HTMLAttributes<HTMLDivElement>, AreaProps>
> = ({ direction, flex, children, ...props }) => {
  return (
    <div className={classNames("TabArea", direction, { flex })} {...props}>
      {children}
    </div>
  );
};

export default Area;
