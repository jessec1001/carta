import classNames from "classnames";
import { ComponentProps, FunctionComponent } from "react";
import { Notifications } from "components/notifications";
import Header from "./Header";
import Footer from "./Footer";
import "./layout.css";

/** The props used for the {@link PageLayout} component. */
interface PageLayoutProps extends ComponentProps<"div"> {
  /** Whether a header navigation bar should be included in the layout. */
  header?: boolean;
  /** Whether a footer should be included in the layout. */
  footer?: boolean;
}

/** A component that adds typically visible components such as a header and footer to a page. */
const PageLayout: FunctionComponent<PageLayoutProps> = ({
  header,
  footer,
  children,
  className,
  ...props
}) => {
  return (
    <div className={"layout-container"}>
      <Notifications.Dock />
      {header && <Header />}
      <div className={classNames("layout-content", className)} {...props}>
        {children}
      </div>
      {footer && <Footer />}
    </div>
  );
};

export default PageLayout;
export type { PageLayoutProps };
