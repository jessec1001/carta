import { ComponentProps, FC } from "react";
import classNames from "classnames";
import IconButton from "./IconButton";
import styles from "./IconButton.module.css";

/** An icon-based button that represents adding or creating an object. */
const IconButtonAdd: FC<ComponentProps<typeof IconButton>> = ({
  className,
  ...props
}) => {
  return (
    <IconButton
      className={classNames(styles.scaled, styles.typeAdd, className)}
      {...props}
    />
  );
};

export default IconButtonAdd;
