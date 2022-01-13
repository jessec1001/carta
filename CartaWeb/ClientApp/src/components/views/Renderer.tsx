import { FC } from "react";
import ViewContext, { useViews } from "./Context";
import { Tabs } from "components/tabs";

// TODO: Cleanup import.
import { SplitArea } from "components/containers/split";

// TODO: Implement a rendering system where there may be multiple root-like components. Then, based on some query
//       parameter in the URL of the webpage (i.e. '?view=0'), we can render a different view hierarchy.

// TODO: Add documentation to this component.

/** The props used for the {@link Renderer} component. */
interface RendererProps {
  /** An optionally specified root view identifier. */
  root?: number;
}

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
      // If we could not get the children views, we stop attempting to render.
      if (childViews === null) return null;

      return (
        // TODO: Incorporate tab focus.
        // TODO: Try to move the tab bar up into a status bar of the split panels.
        // TODO: Make sure that the view is closed when the tab 'x' button is clicked (if closeable).
        <Tabs
          activeTab={view.activeId}
          onChangeTab={(childId) => actions.activateView(childId as number)}
        >
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
                      onClose={() => actions.removeView(childView.currentId)}
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
                    <Renderer />
                  </Tabs.Panel>
                </ViewContext.Provider>
              );
            })}
          </Tabs.Area>
        </Tabs>
      );
    default:
      return null;
  }
};

export default Renderer;
