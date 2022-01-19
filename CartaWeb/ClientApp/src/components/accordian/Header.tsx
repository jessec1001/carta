import { FunctionComponent, HTMLAttributes } from "react";
import classNames from "classnames";
import styles from "./Accordian.module.css";

/** The props used for the {@link Header} component. */
interface HeaderProps extends HTMLAttributes<HTMLDivElement> {}

/** The header for an accordian. */
const Header: FunctionComponent<HeaderProps> = ({
  children,
  className,
  ...props
}) => {
  return (
    <div className={classNames(styles.header, className)} {...props}>
      {children}
    </div>
  );
};

export default Header;
export type { HeaderProps };
