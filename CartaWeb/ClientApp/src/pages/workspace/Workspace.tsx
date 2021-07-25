import { PageLayout } from "components/layout";
import { DatasetListView } from "components/views";
import { Workspace, WorkspaceAPI } from "library/api";
import { FunctionComponent, useEffect, useRef, useState } from "react";
import { useLocation } from "react-router";

const tips: string[] = [
  "Every operation in Carta does not mutate input data.",
  "You can integrate with federated data providers such as HyperThought™.",
  "Customizing your workspace can help you be more productive.",
  "You can invert a selection to negate a certain condition.",
  "You can view multiple datasets simultaneously by dragging tabs to other portions of the screen.",
  "Opening the same workspace in multiple tabs or windows automatically synchronizes operations between them.",
];

const WorkspacePage: FunctionComponent = () => {
  const tipInterval = 5000;

  const workspaceApiRef = useRef(new WorkspaceAPI());
  const workspaceApi = workspaceApiRef.current;

  const id = new URLSearchParams(useLocation().search).get("id") as string;
  // const { id } =  useQuery<{ id: string }>();

  const [tip, setTip] = useState("");
  const [loading, setLoading] = useState(true);
  const [loadingStep, setLoadingStep] = useState("");
  const [loadingProgress, setLoadingProgress] = useState(0);

  const [workspace, setWorkspace] = useState<Workspace | null>(null);

  useEffect(() => {
    (async () => {
      const timeBefore = Date.now();

      setLoadingStep("Loading workspace");
      setWorkspace(await workspaceApi.getCompleteWorkspace(id));
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
      {loading && (
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            flexGrow: 1,
            alignItems: "center",
            backgroundColor: "var(--color-fill-overlay)",
            color: "var(--color-stroke-lowlight)",
          }}
        >
          <div
            style={{
              width: "100%",
              height: "4px",
              background: `linear-gradient(to right, var(--color-primary) ${
                loadingProgress * 100
              }%, var(--color-secondary) ${loadingProgress * 100}%)`,
            }}
          ></div>
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
                  color: "var(--color-stroke-faint)",
                }}
              >
                {loadingStep}
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
      {!loading && workspace?.datasets && (
        <DatasetListView datasets={workspace.datasets} />
      )}
    </PageLayout>
  );
};

export default WorkspacePage;
