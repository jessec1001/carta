import { ComponentProps, FC, useEffect } from "react";
import classNames from "classnames";
import { useSplits } from "./Context";
import styles from "./Splits.module.css";

/** The props used for the {@link Panel} component. */
interface PanelProps extends Omit<ComponentProps<"div">, "id"> {
  /** The unique identifier of the split that this component corresponds to. */
  id: string | number;
  /** The size of this split. If not specified, size will be distributed evenly along remaining splits. */
  ratio?: number;
}

/** A component that contains the content for a split. */
const Panel: FC<PanelProps> = ({
  id,
  ratio,
  className,
  style,
  children,
  ...props
}) => {
  // Get the state of the splits and get the size of this split.
  const { actions } = useSplits();
  const size = actions.get(id);

  // We need to setup and tear down the ratio of the split.
  useEffect(() => {
    actions.set(id, ratio || null);
    return () => actions.unset(id);
  }, [actions, id, ratio]);

  // We return the split panel with a special style applied to indicate the size of the split.
  return (
    <div
      className={classNames(className)}
      style={{ ...style, ["--size" as any]: size }}
      {...props}
    >
      {children}
    </div>
  );
};

export default Panel;
export type { PanelProps };
