import { AppColors } from "app";
import { FC } from "react";
import {
  Link as RouterLink,
  LinkProps as RouterLinkProps,
} from "react-router-dom";
import styles from "./Link.module.css";
import fgStyles from "styles/foreground.module.css";
import classNames from "classnames";

/** The props used for the {@link Link} component. */
interface LinkProps extends RouterLinkProps {
  /** The color of the link. Defaults to primary. */
  color?: AppColors;
}

/** A component that renders a hyperlink. */
const Link: FC<LinkProps> = ({
  color = "primary",
  className,
  children,
  ...props
}) => {
  return (
    <RouterLink
      className={classNames(styles.link, fgStyles[color], className)}
      {...props}
    >
      {children}
    </RouterLink>
  );
};

export default Link;
export type { LinkProps };
