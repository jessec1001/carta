import { ComponentProps, FC } from "react";
import classNames from "classnames";
import styles from "./Card.module.css";

/** The props used for the {@link Footer} component. */
interface FooterProps extends ComponentProps<"div"> {}

/** A component that renders the footer of a {@link Card} component. */
const Footer: FC<FooterProps> = ({ className, children, ...props }) => {
  return (
    <div className={classNames(styles.footer, className)} {...props}>
      {children}
    </div>
  );
};

export default Footer;
