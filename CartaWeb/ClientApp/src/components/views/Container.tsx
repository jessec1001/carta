import classNames from "classnames";
import {
  forwardRef,
  HTMLAttributes,
  PropsWithChildren,
  useEffect,
} from "react";
import { Modify } from "types";
import { useViews } from "./Context";

import "./Container.css";

/** The props used for the {@link Container} component. */
interface ContainerProps {
  /** The title of the view to display in the tab for the view. */
  title?: React.ReactNode;
  /** Whether the view should be closeable or not. Defaults to true. */
  closeable?: boolean;

  /** Whether the view container should be padded. */
  padded?: boolean;
  /** The direction that the view container should be scrollable or filled. */
  direction?: "horizontal" | "vertical" | "fill";
}

/** A container for a particular view in a views context. */
const Container = forwardRef<
  HTMLDivElement,
  PropsWithChildren<Modify<HTMLAttributes<HTMLDivElement>, ContainerProps>>
>(({ title, closeable, padded, direction, children }, ref) => {
  // TODO: Add status for tabs.
  // We set the options on the view when specified.
  const { actions } = useViews();
  useEffect(() => {
    actions.setOptions({ title, closeable });
  }, [title, closeable, actions]);

  return (
    <div
      ref={ref}
      className={classNames("View-Container", { padded }, direction)}
    >
      <div className={"View-Container-Internal"}>{children}</div>
    </div>
  );
});

export default Container;
