import { FunctionComponent } from "react";
import { Navigation } from "./header";
import { Footer } from "./footer";

import "./layout.css";

/** The props used for the {@link Layout} component. */
interface LayoutProps {
  /** Whether a navigation bar should be included in the layout. */
  navigation?: boolean;
  /** Whether a footer should be included in the layout. */
  footer?: boolean;
}

/** A component that adds typically visible components such as a header and footer to a page. */
const Layout: FunctionComponent<LayoutProps> = ({
  navigation,
  footer,
  children,
  ...props
}) => {
  return (
    <div className="layout-container" {...props}>
      {navigation && <Navigation />}
      <div className="layout-content">{children}</div>
      {footer && <Footer />}
    </div>
  );
};

export default Layout;
