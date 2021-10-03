import React, { FunctionComponent, HTMLAttributes } from "react";
import classNames from "classnames";

import "./tab.css";
import { Modify } from "types";

/** The props used for the {@link TabBar} component. */
interface TabBarProps {
  /** The direction that the tab bar should stretch. */
  direction: "horizontal" | "vertical";
}

/** The bar containing interactive tab bar buttons. */
const TabBar: FunctionComponent<
  Modify<HTMLAttributes<HTMLDivElement>, TabBarProps>
> = ({ direction, children, ...props }) => {
  return (
    <div className={classNames("tab-bar", direction)} {...props}>
      {children}
    </div>
  );
};

export default TabBar;
