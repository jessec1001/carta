import classNames from "classnames";
import { LoadingIcon } from "components/icons";
import { ComponentProps, FC } from "react";
import styles from "./EditorNode.module.css";

/** The props used for the {@link EditorNode} component. */
interface EditorNodeProps extends ComponentProps<"div"> {
  loading?: boolean;
  selected?: boolean;
}
/** A component that renders a rectangular node in the workflow editor. */
const EditorNode: FC<EditorNodeProps> = ({
  loading,
  selected,
  className,
  children,
  ...props
}) => {
  return (
    <div
      className={classNames(
        styles.node,
        { [styles.selected]: selected, [styles.loading]: loading },
        className
      )}
      {...props}
    >
      {loading && <LoadingIcon />}
      {children}
    </div>
  );
};

export default EditorNode;
export type { EditorNodeProps };
