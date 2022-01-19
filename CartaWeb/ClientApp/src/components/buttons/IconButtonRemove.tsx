import { ComponentProps, FC } from "react";
import classNames from "classnames";
import IconButton from "./IconButton";
import styles from "./IconButton.module.css";

/** An icon-based button that represents removing or deleting an object. */
const IconButtonRemove: FC<ComponentProps<typeof IconButton>> = ({
  className,
  ...props
}) => {
  return (
    <IconButton
      className={classNames(styles.scaled, styles.typeRemove, className)}
      {...props}
    />
  );
};

export default IconButtonRemove;
