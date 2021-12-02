import { FunctionComponent, useEffect } from "react";
import { useViews } from "./Context";

/** The props used for the {@link Container} component. */
interface ContainerProps {
  /** The title of the view to display in the tab for the view. */
  title?: React.ReactNode;
  /** Whether the view should be closeable or not. Defaults to true. */
  closeable?: boolean;
}

/** A container for a particular view in a views context. */
const Container: FunctionComponent<ContainerProps> = ({
  title,
  closeable,
  children,
}) => {
  // TODO: Add status for tabs.
  // We set the options on the view when specified.
  const { actions } = useViews();
  useEffect(() => {
    actions.setOptions({ title, closeable });
  }, [title, closeable, actions]);

  return <>{children}</>;
};

export default Container;
