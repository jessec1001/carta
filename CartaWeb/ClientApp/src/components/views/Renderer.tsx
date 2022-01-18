import { FC, useState } from "react";
import ViewContext, { useViews } from "./Context";
import { Tabs } from "components/tabs";

// TODO: Cleanup import.
import { SplitArea } from "components/containers/split";
import { TabView, View } from ".";

// TODO: Implement a rendering system where there may be multiple root-like components. Then, based on some query
//       parameter in the URL of the webpage (i.e. '?view=0'), we can render a different view hierarchy.

// TODO: Add documentation to this component.

/** The props used for the {@link Renderer} component. */
interface RendererProps {
  /** An optionally specified root view identifier. */
  root?: number;
}

const TabPanelRenderer: FC = ({ children }) => {
  const { viewId, actions: viewActions } = useViews();
  const [dragging, setDragging] = useState(false);

  return (
    <div
      style={{
        position: "relative",
        display: "flex",
        flex: "1 1 0",
      }}
    >
      <div
        style={{
          position: "absolute",
          width: "8rem",
          height: "100%",
          top: "0%",
          right: "0%",
          backgroundColor: dragging
            ? "var(--color-primary-select)"
            : "transparent",
          zIndex: 100,
        }}
        onDragEnter={(event) => {
          if (event.dataTransfer.types.includes("text/json+tab"))
            setDragging(true);
        }}
        onDragLeave={(event) => {
          if (event.dataTransfer.types.includes("text/json+tab"))
            setDragging(false);
        }}
        onDrop={(event) => {
          if (event.dataTransfer.types.includes("text/json+tab")) {
            // Try to find the nearest parent of this view that is a tab view.
            let parentTabView: TabView | null = null;
            let parentView: View | null = viewActions.getParentView(viewId);
            while (true) {
              if (parentView === null) break;
              if (parentView.type === "tab") {
                parentTabView = parentView as TabView;
                break;
              }
              // TODO: Consider changing 'currentId' to either 'id' or 'viewId'.
              parentView = viewActions.getParentView(parentView.currentId);
            }

            // If the parent tab view is found, then we create a new split view.
            // We drop both the parent tab view and the dropped view into the new split view.
            if (parentTabView !== null) {
              const dropId = parseInt(
                event.dataTransfer.getData("text/json+tab")
              );

              // Simpify creating new views.
              const splitViewId = viewActions.addView({
                type: "split",
                childIds: [parentTabView.currentId, dropId],
                direction: "horizontal",
                sizes: [0.5, 0.5],
                title: null,
                closeable: false,
                status: "none",
                currentId: 0,
                parentId: parentTabView.parentId,
                tags: {},
              });
              viewActions.setView(dropId, (dropView) => ({
                ...dropView,
                parentId: splitViewId,
              }));
              viewActions.setView(parentTabView.currentId, (parentView) => ({
                ...parentView,
                parentId: splitViewId,
              }));
            }
          }
        }}
      />
      {children}
    </div>
  );
};

/** A component that renders a tab view. */
const TabRenderer: FC<{ view: TabView }> = ({ view }) => {
  // Get the child views.
  const { rootId, actions } = useViews();
  const childViews = actions.getChildViews(view.currentId);

  // If we could not get the children views, we stop attempting to render.
  if (childViews === null) return null;

  // Construct a subcontext.
  const contextActions = {
    getView: actions.getView,
    setView: actions.setView,
    addView: actions.addView,
    removeView: actions.removeView,
    getHistory: actions.getHistory,
    addHistory: actions.addHistory,
  };

  // These are event handlers that translate from tab events to view events.
  const handleCloseTab = (id: number) => {
    actions.removeView(id);
  };
  const handleChangeTab = (id: string | number | null) => {
    actions.activateView(id as number);
  };
  const handleDragTab = (
    sourceId: string | number | null,
    targetId: string | number | null
  ) => {
    // Get the source and target child indices.
    let sourceIndex = childViews.findIndex((childView) => {
      return childView?.currentId === sourceId;
    });
    let targetIndex = childViews.findIndex((childView) => {
      return childView?.currentId === targetId;
    });

    // We move the source to before the target.
    actions.setView(view.currentId, (view) => {
      // If the target is null, we move the source to the end.
      if (view.type !== "tab") return view;
      if (targetIndex === -1) targetIndex = view.childIds.length;

      // Move the source view to before the target view.
      const childIds: number[] = [];
      for (let k = 0; k <= view.childIds.length; k++) {
        if (k === sourceIndex) continue;
        if (k === targetIndex) childIds.push(sourceId as number);
        if (k < view.childIds.length) childIds.push(view.childIds[k]);
      }
      return {
        ...view,
        childIds,
      };
    });
  };

  return (
    // TODO: Try to move the tab bar up into a status bar of the split panels.
    <Tabs
      draggableTabs
      activeTab={view.activeId}
      onChangeTab={handleChangeTab}
      onDragTab={handleDragTab}
    >
      {/* TODO: Allow for changing the direction of the tabs. */}
      <Tabs.Area direction="horizontal" flex>
        <Tabs.Bar>
          {childViews.map((childView) => {
            if (!childView) return null;
            return (
              // We use a view context for the tab bar to allow for view-based tab buttons.
              <ViewContext.Provider
                key={childView.currentId}
                value={{
                  viewId: childView.currentId,
                  rootId: rootId,
                  ...contextActions,
                }}
              >
                <Tabs.Tab
                  id={childView.currentId}
                  closeable={childView.closeable}
                  status={childView.status}
                  onClose={() => handleCloseTab(childView.currentId)}
                >
                  {childView.title}
                </Tabs.Tab>
              </ViewContext.Provider>
            );
          })}
        </Tabs.Bar>
        {childViews.map((childView) => {
          if (!childView) return null;
          return (
            // We use a view context for the tab content to allow the content to access the view hierarchy.
            <ViewContext.Provider
              key={childView.currentId}
              value={{
                viewId: childView.currentId,
                rootId: rootId,
                ...contextActions,
              }}
            >
              <Tabs.Panel id={childView.currentId}>
                {/* <TabPanelRenderer> */}
                <Renderer />
                {/* </TabPanelRenderer> */}
              </Tabs.Panel>
            </ViewContext.Provider>
          );
        })}
      </Tabs.Area>
    </Tabs>
  );
};

const Renderer: FC<RendererProps> = () => {
  // Get relevant view information.
  const { viewId, rootId, actions } = useViews();
  const view = actions.getView(viewId);
  const childViews = actions.getChildViews(viewId);

  // If the view exists, render it.
  const contextActions = {
    getView: actions.getView,
    setView: actions.setView,
    addView: actions.addView,
    removeView: actions.removeView,
    getHistory: actions.getHistory,
    addHistory: actions.addHistory,
  };
  if (!view) return null;
  // TODO: Break these up into separate subcomponents.
  switch (view.type) {
    case "element":
      return (
        <ViewContext.Provider
          key={view.currentId}
          value={{ viewId, rootId, ...contextActions }}
        >
          {view.element}
        </ViewContext.Provider>
      );

    case "split":
      // If we could not get the children views, we stop attempting to render.
      if (childViews === null) return null;

      return (
        // TODO: Utilize a context for split panels.
        // TODO: Incorporate view sizes.
        <SplitArea direction={view.direction}>
          {childViews.map((childView) => {
            if (!childView) return null;
            return (
              <ViewContext.Provider
                key={childView.currentId}
                value={{
                  viewId: childView.currentId,
                  rootId: rootId,
                  ...contextActions,
                }}
              >
                <Renderer />
              </ViewContext.Provider>
            );
          })}
        </SplitArea>
      );

    case "tab":
      return <TabRenderer view={view} />;
    default:
      return null;
  }
};

export default Renderer;
