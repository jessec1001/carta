import { FunctionComponent, HTMLAttributes } from "react";

import "./Bar.css";

/** The bar containing interactive tab bar buttons. */
const Bar: FunctionComponent<HTMLAttributes<HTMLDivElement>> = ({
  children,
  ...props
}) => {
  return (
    <div className="TabBar" {...props}>
      <div className="TabBar-Wrapper">{children}</div>
    </div>
  );
};

export default Bar;
