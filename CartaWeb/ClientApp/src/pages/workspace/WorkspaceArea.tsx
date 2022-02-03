import { FC, useCallback, useEffect, useMemo, useRef } from "react";
import { useLocation } from "react-router";
import {
  useAPI,
  useLoading,
  useRefresh,
  useStoredState,
  useTicker,
} from "hooks";
import { Workspace } from "library/api";
import { PageLayout } from "components/layout";
import { OperationsListView } from "pages/workspace/views";
import WorkspaceWrapper from "components/workspace/WorkspaceWrapper";
import WorkspaceToolbar from "components/workspace/WorkspaceToolbar";
import { Loading, Text } from "components/text";
import { useViews } from "components/views";
import { Views } from "components/views";
import styles from "./WorkspaceArea.module.css";

// TODO: There should be preset layouts available for this page.
/** A page-specific component that is used to initialize the default layout of this page. */
const DefaultLayout: FC = () => {
  // Get information about the views context to use for initialization.
  const initializedRef = useRef<boolean>(false);
  const { rootId, actions } = useViews();

  // Initialize the views context.
  useEffect(() => {
    // Only initialize the views context if it has not been initialized yet.
    const initialized = initializedRef.current;
    initializedRef.current = true;

    // We add all of the necessary views to construct the default layout..
    if (!initialized) {
      actions.addElementToContainer(rootId, <OperationsListView />, true);
    }
  }, [rootId, actions]);

  // We don't actually render any visible children from this subcomponent.
  return null;
};

/** A page-specific component that renders an overlay over the entire page. */
const Overlay: FC = ({ children }) => {
  // We setup the ticker so that we occassionally fetch one of a collection of tips.
  const tips: string[] = [
    "Every operation in Carta does not mutate input data.",
    "You can integrate with federated data providers such as HyperThought™.",
    "Customizing your workspace can help you be more productive.",
    "You can invert a selection to negate a certain condition.",
    "You can view multiple datasets simultaneously by dragging tabs to other portions of the screen.",
    "Opening the same workspace in multiple tabs or windows automatically synchronizes operations between them.",
  ];
  const tipDisplayInterval = 4000;
  const tip = useTicker(tips, true, tipDisplayInterval);

  return (
    <div className={styles.overlay}>
      <div className={styles.overlayContent}>{children}</div>
      <p className={styles.overlayTip}>{tip}</p>
    </div>
  );
};

/** A page-specific component that renders loading content into the overlay. */
const LoadingOverlay: FC<{ text: string; progress: number }> = ({
  text,
  progress,
}) => {
  // TODO: Add a loading/progress bar.
  //       This might be attached to a status element of the workspace area or it could be relocated near the loading text.

  return (
    <>
      <Text size="medium">Your workspace is loading</Text>
      <Loading color="muted">{text}</Loading>
    </>
  );
};
/** A page-specific component that renders error content into the overlay. */
const ErrorOverlay: FC<{ error: string }> = ({ error }) => {
  return (
    <>
      <Text size="medium" color="error">
        An error occurred
      </Text>
      <Text color="error">{error}</Text>
    </>
  );
};

/** The page users navigate to in order to see a particular */
const WorkspacePage: FC = () => {
  // Setup the loading stages.
  const {
    findCurrentStage,
    progressStage,
    completeStage,
    computeTotalProgress,
    isLoadingComplete,
  } = useLoading([
    { text: "Loading workspace", weight: 4 },
    {
      text: "Finishing up",
      weight: 1,
      callback: () => setTimeout(() => completeStage(1), 2500),
    },
  ]);
  const [loadingStage] = findCurrentStage();
  const loadingText = loadingStage?.text ?? "Loading";
  const loadingProgress = computeTotalProgress();

  // Grab information about the search query.
  const search = useLocation().search;
  const searchParams = useMemo(() => new URLSearchParams(search), [search]);

  // Grab the workspace ID from the URL. Note that this may be null.
  // Then, we grab the workspace from the API. We do so in a way so that we can retrieve any error that may occur.
  const { workspaceAPI } = useAPI();
  const workspaceId = searchParams.get("id");
  const workspaceFetcher = useCallback(async () => {
    progressStage(0, 0.0);
    let workspace: Workspace;
    if (!workspaceId) throw new Error("No workspace ID was provided.");
    else workspace = await workspaceAPI.getWorkspace(workspaceId);
    completeStage(0);
    return workspace;
  }, [completeStage, progressStage, workspaceAPI, workspaceId]);
  const [workspace, workspaceError] = useRefresh<Workspace | undefined>(
    workspaceFetcher
  );

  // Whenever a workspace is loaded, we want to update the title on the page.
  useEffect(() => {
    if (!workspace) document.title = `Workspace - Carta`;
    else document.title = `${workspace.name} - Carta`;
  }, [workspace]);

  // Update the latest accession to this workspace.
  const [, setWorkspaceHistory] = useStoredState<Record<string, number>>(
    {},
    "workspaceHistory"
  );
  useEffect(() => {
    if (!workspaceId) return;
    setWorkspaceHistory((history) => ({
      ...history,
      [workspaceId]: Date.now(),
    }));
  }, [workspaceId, setWorkspaceHistory]);

  return (
    <PageLayout header>
      <div className={styles.overlayContainer}>
        {workspace && (
          <WorkspaceWrapper workspace={workspace}>
            <Views>
              <WorkspaceToolbar />
              <DefaultLayout />
              <Views.Renderer />
            </Views>
          </WorkspaceWrapper>
        )}
        {!isLoadingComplete && (
          <Overlay>
            {!workspaceError && (
              <LoadingOverlay text={loadingText} progress={loadingProgress} />
            )}
            {workspaceError && <ErrorOverlay error={workspaceError.message} />}
          </Overlay>
        )}
      </div>
    </PageLayout>
  );
};

export default WorkspacePage;
