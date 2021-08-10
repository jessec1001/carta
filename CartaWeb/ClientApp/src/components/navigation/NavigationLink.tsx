import { FunctionComponent } from "react";
import { Link, LinkProps } from "react-router-dom";
import classNames from "classnames";

/** The props used for the {@link NavigationLink} component. */
interface NavigationLinkProps {
  /** Whether to display the link in lowercase stylistically. */
  lower?: boolean;
}

/* A component that represents a navigation link. */
const NavigationLink: FunctionComponent<LinkProps & NavigationLinkProps> = ({
  children,
  lower,
  ...props
}) => {
  return (
    <Link className={classNames("nav-link", { lower })} {...props}>
      {children}
    </Link>
  );
};

export default NavigationLink;
export type { NavigationLinkProps };
