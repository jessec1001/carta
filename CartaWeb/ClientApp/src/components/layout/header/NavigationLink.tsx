import { FunctionComponent } from "react";
import { Link, LinkProps } from "react-router-dom";

/* A component that represents a navigation link. */
const NavigationLink: FunctionComponent<LinkProps> = ({
  children,
  ...props
}) => {
  return (
    <Link className="nav-link" {...props}>
      {children}
    </Link>
  );
};

export default NavigationLink;
