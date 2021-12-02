import { FunctionComponent } from "react";

import "./Accordian.css";

/** The props used for the {@link Header} component. */
interface HeaderProps {}

/** The header for an accordian. */
const Header: FunctionComponent<HeaderProps> = ({ children }) => {
  return <div className={"Accordian-Header"}>{children}</div>;
};

export default Header;
export type { HeaderProps };
