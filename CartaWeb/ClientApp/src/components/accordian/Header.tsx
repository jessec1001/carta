import { FunctionComponent } from "react";

import "./Accordian.css";

/** The props used for the {@link Header} component. */
interface HeaderProps {}

const Header: FunctionComponent<HeaderProps> = ({ children }) => {
  return <div className="accordian-header">{children}</div>;
};

export default Header;
export type { HeaderProps };
