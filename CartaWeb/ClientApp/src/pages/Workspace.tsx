import { DatasetIcon } from "components/icons";
import { Layout } from "components/layout";
import { Workspace, WorkspaceAPI } from "library/api";
import { FunctionComponent, useEffect, useRef, useState } from "react";
import { useParams } from "react-router";

const tips: string[] = [
  "Every operation in Carta does not mutate input data.",
  "You can integrate with federated data providers such as HyperThought&trade;",
  "Customizing your workspace can help you be more productive.",
  "You can invert a selection to negate a certain condition.",
];

const WorkspacePage: FunctionComponent = () => {
  const tipInterval = 5000;

  const workspaceApiRef = useRef(new WorkspaceAPI());
  const workspaceApi = workspaceApiRef.current;

  const { id } = useParams<{ id: string }>();

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

  console.log(workspace, workspace?.datasets);

  return (
    <Layout header>
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
      {!loading && workspace && (
        <div
          style={{
            flexGrow: 1,
          }}
        >
          <p>Datasets</p>
          <ul>
            {workspace.datasets &&
              workspace.datasets.map((dataset) => (
                <li>
                  <span
                    className="normal-text"
                    style={{
                      color: "var(--color-stroke-lowlight)",
                    }}
                  >
                    <DatasetIcon />
                    {dataset.name ?? `(${dataset.source}/${dataset.resource})`}
                  </span>
                </li>
              ))}
          </ul>
        </div>
      )}
    </Layout>
  );
};

export default WorkspacePage;
