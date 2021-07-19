import { FunctionComponent } from "react";
import {
  Link as RouterLink,
  LinkProps as RouterLinkProps,
} from "react-router-dom";

import "./link.css";

/** A component that renders a link in the Carta theme. */
const Link: FunctionComponent<RouterLinkProps> = ({ children, ...props }) => {
  return (
    <RouterLink className="link" {...props}>
      {children}
    </RouterLink>
  );
};

export default Link;
