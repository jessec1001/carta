import React, { FunctionComponent, HTMLAttributes } from "react";
import classNames from "classnames";
import { Modify } from "types";
import { useTabs } from "./Context";
import "./Tab.css";
import { CloseButton } from "components/buttons";

/**
 * The props used for the {@link Tab} component.
 */
interface TabProps {
  /** The unique identifier of the tab that this component corresponds to. */
  id: string | number;

  /** The status indicator used to decorate the tab navigation bar. */
  status?: "none" | "modified" | "unmodified" | "info" | "warning" | "error";

  /** Whether the tab is closeable by the user. */
  closeable?: boolean;

  /** The event handler for when the tab has requested to close. */
  onClose?: () => void;
  /** The event handler for when the tab has been focused. */
  onFocus?: () => void;
}

/** A component that renders a tab within a tab container with specified properties on the tab button. */
const Tab: FunctionComponent<
  Modify<HTMLAttributes<HTMLDivElement>, TabProps>
> = ({
  id,
  status,
  closeable,
  onClose = () => {},
  onFocus = () => {},
  onClick = () => {},
  onDragStart = () => {},
  onDragEnd = () => {},
  onDragOver = () => {},
  children,
  ...props
}) => {
  // We use the state of the tabs context to perform some rendering and functions of this component.
  const {
    draggableTabs,
    setDragSource,
    setDragTarget,
    finishDrag,
    dragTarget,
    activeTab,
    actions: tabActions,
  } = useTabs();

  // These event handlers update the tab state while emitting relevant events.
  const handleFocus = (event: React.MouseEvent<HTMLDivElement>) => {
    tabActions.set(id);
    onClick(event);
    onFocus();
  };
  const handleClose = () => {
    if (activeTab === id) tabActions.clear();
    onClose();
  };

  // These event handlers deal with dragging the tab.
  const handleDragStart = (event: React.DragEvent<HTMLDivElement>) => {
    if (!draggableTabs) return;

    // Add data to the drag event for the tab.
    event.dataTransfer.setData("text/json+tab", JSON.stringify(id));
    event.dataTransfer.effectAllowed = "move";
    setDragSource(id);

    // Emit the drag start event.
    onDragStart(event);
  };
  const handleDragEnd = (event: React.DragEvent<HTMLDivElement>) => {
    if (!draggableTabs) return;
    finishDrag();

    // Emit the drag end event.
    onDragEnd(event);
  };
  const handleDragOver = (event: React.DragEvent<HTMLDivElement>) => {
    if (!draggableTabs) return;

    // Prevent the propagation of the drag event so that the tab bar does not become the drag target.
    event.stopPropagation();
    setDragTarget(id);

    // Emit the drag over event.
    onDragOver(event);
  };

  // How to deal with tab dragging?
  // When dragging over a tab, the tab will be moved to either the left or right of the tab.
  // When dragging over the tab bar, the tab will be moved to the end of the tab bar.

  return (
    <div
      className={classNames("Tab", `status-${status}`, {
        active: activeTab === id,
        closeable: closeable,
        drag: draggableTabs && dragTarget === id,
      })}
      draggable={draggableTabs}
      onClick={handleFocus}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      onDragOver={handleDragOver}
      {...props}
    >
      <div className="Tab-Content">{children}</div>
      {closeable && <CloseButton onClick={handleClose} />}
    </div>
  );
};

export default Tab;
export type { TabProps };
