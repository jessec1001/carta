import { ComponentProps, FC } from "react";
import classNames from "classnames";
import styles from "./Card.module.css";

/** The props used for the {@link Body} component. */
interface BodyProps extends ComponentProps<"div"> {}

/** A component that renders the body of a {@link Card} component. */
const Body: FC<BodyProps> = ({ className, children, ...props }) => {
  return (
    <div className={classNames(styles.body, className)} {...props}>
      {children}
    </div>
  );
};

export default Body;
