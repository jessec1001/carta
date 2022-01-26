import { FunctionComponent, useEffect, useRef, useState } from "react";
import { useLocation } from "react-router";
import { WorkspaceAPI } from "library/api";
import { PageLayout } from "components/layout";
import { OperationsListView } from "components/workspace/views";
import WorkspaceWrapper from "components/workspace/WorkspaceWrapper";
import WorkspaceToolbar from "components/workspace/WorkspaceToolbar";
import { Loading } from "components/text";
import { useStoredState } from "hooks";
import { useViews } from "components/views";
import { Views } from "components/views";

const tips: string[] = [
  "Every operation in Carta does not mutate input data.",
  "You can integrate with federated data providers such as HyperThought™.",
  "Customizing your workspace can help you be more productive.",
  "You can invert a selection to negate a certain condition.",
  "You can view multiple datasets simultaneously by dragging tabs to other portions of the screen.",
  "Opening the same workspace in multiple tabs or windows automatically synchronizes operations between them.",
];

const WorkspacePageDefaultLayout: FunctionComponent = () => {
  const { rootId, actions } = useViews();
  const initializedRef = useRef<boolean>(false);

  useEffect(() => {
    if (!initializedRef.current) {
      initializedRef.current = true;
      actions.addElementToContainer(rootId, <OperationsListView />, true);
    }
  }, [rootId, actions]);

  return null;
};

// TODO: There should be preset layouts available for this page.
const WorkspacePage: FunctionComponent = () => {
  // TODO: Revert to 5000
  const tipInterval = 5000;

  const workspaceApiRef = useRef(new WorkspaceAPI());
  const workspaceApi = workspaceApiRef.current;

  const id = new URLSearchParams(useLocation().search).get("id") as string;
  // const { id } =  useQuery<{ id: string }>();

  const [tip, setTip] = useState("");
  const [loading, setLoading] = useState(true);
  const [loadingStep, setLoadingStep] = useState("");
  const [loadingProgress, setLoadingProgress] = useState(0);

  // const [workspace, setWorkspace] = useState<Workspace | null>(null);

  const [accessions, setAccessions] = useStoredState<Record<string, number>>(
    {},
    "workspaceAccessions"
  );
  useEffect(() => {
    setAccessions({ ...accessions, [id]: Date.now() });
  }, [accessions, id, setAccessions]);

  useEffect(() => {
    (async () => {
      const timeBefore = Date.now();

      setLoadingStep("Loading workspace");
      // setWorkspace(await workspaceApi.getCompleteWorkspace(id));
      setLoadingStep("Finishing up");
      setLoadingProgress(1.0);

      const timeAfter = Date.now();

      setTimeout(
        () => setLoading(false),
        Math.max(0, tipInterval - (timeAfter - timeBefore))
      );
    })();
  }, [workspaceApi, id]);

  useEffect(() => {
    const handleChangeTip = () => {
      setTip(tips[Math.floor(Math.random() * tips.length)]);
    };

    if (loading) {
      const changeTipIntervalId = setInterval(handleChangeTip, tipInterval);
      handleChangeTip();
      return () => clearInterval(changeTipIntervalId);
    }
  }, [loading]);

  return (
    <PageLayout header>
      <WorkspaceWrapper id={id}>
        <Views>
          <div
            style={{
              flex: 1,
              width: "100%",
              display: "flex",
              flexDirection: "column",
            }}
          >
            <WorkspaceToolbar />
            <WorkspacePageDefaultLayout />
            <Views.Renderer />
            <div
              style={{
                width: "100%",
                height: "4px",
                background: `linear-gradient(to right, var(--color-primary) ${
                  loadingProgress * 100
                }%, var(--color-secondary) ${loadingProgress * 100}%)`,
              }}
            ></div>
          </div>
        </Views>
      </WorkspaceWrapper>
      {loading && (
        <div
          style={{
            position: "absolute",
            top: "0",
            left: "0",
            width: "100%",
            height: "100%",
            display: "flex",
            flexDirection: "column",
            flexGrow: 1,
            alignItems: "center",
            backgroundColor: "var(--color-fill-overlay)",
            color: "var(--color-stroke-lowlight)",
            zIndex: 100,
          }}
        >
          <div
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              flexGrow: 1,
              width: "100%",
            }}
          >
            <div
              style={{
                textAlign: "center",
              }}
            >
              <p
                style={{
                  fontSize: "1.4rem",
                }}
              >
                Your workspace is loading.
              </p>
              <p
                style={{
                  color: "var(--color-stroke-muted)",
                }}
              >
                <Loading text={loadingStep} />
              </p>
            </div>
          </div>
          <div
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              width: "100%",
              padding: "2rem 0rem",
            }}
          >
            <p>{tip}</p>
          </div>
        </div>
      )}
    </PageLayout>
  );
};

export default WorkspacePage;
