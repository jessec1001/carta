import { ComponentProps, FC } from "react";
import classNames from "classnames";
import styles from "./Card.module.css";

/** The props used for the {@link Header} component. */
interface HeaderProps extends ComponentProps<"div"> {}

/** A component that renders the header of a {@link Card} component. */
const Header: FC<HeaderProps> = ({ className, children, ...props }) => {
  return (
    <div className={classNames(styles.header, className)} {...props}>
      {children}
    </div>
  );
};

export default Header;
