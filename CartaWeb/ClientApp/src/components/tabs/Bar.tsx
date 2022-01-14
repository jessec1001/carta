import classNames from "classnames";
import { FunctionComponent, HTMLAttributes } from "react";
import { useTabs } from "./Context";
import "./Bar.css";

/** The bar containing interactive tab bar buttons. */
const Bar: FunctionComponent<HTMLAttributes<HTMLDivElement>> = ({
  children,
  onDragOver = () => {},
  onMouseOut = () => {},
  ...props
}) => {
  // We use the state of the tabs context to perform some rendering and functions of this component.
  const { draggableTabs, setDragTarget, dragSource, dragTarget } = useTabs();

  // These event handlers deal with dragging the tab.
  const handleDragOver = (event: React.DragEvent<HTMLDivElement>) => {
    if (!draggableTabs) return;
    if (event.dataTransfer.types.includes("text/json+tab")) setDragTarget(null);

    // Emit the drag over event.
    onDragOver(event);
  };

  return (
    <div
      className={classNames("TabBar", {
        drag: draggableTabs && dragSource !== null && dragTarget === null,
      })}
      onDragOver={handleDragOver}
      {...props}
    >
      <div className="TabBar-Wrapper">{children}</div>
    </div>
  );
};

export default Bar;
