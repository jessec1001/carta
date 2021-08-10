import { FunctionComponent, useEffect, useMemo, useState } from "react";
import { DataCRUD, useAPI, useCRUD } from "hooks";
import { Workspace, WorkspaceDataset } from "library/api";
import { WorkspaceContext } from "context";

/** The props used for the {@link WorkspaceWrapper} component. */
interface WorkspaceWrapperProps {
  /** The unique identifier for the workspace. */
  id: string;
}

/** A component that wraps its children in a context that exposes all data loaded for a particular workspace. */
const WorkspaceWrapper: FunctionComponent<WorkspaceWrapperProps> = ({
  id,
  children,
}) => {
  // We need the workspace API to access workspace objects.
  const { workspaceAPI } = useAPI();

  // We store the original workspace object itself.
  const [workspace, setWorkspace] = useState<Workspace | null>(null);
  useEffect(() => {
    (async () => {
      const workspace = await workspaceAPI.getWorkspace(id);
      setWorkspace(workspace);
    })();
  }, [id, workspaceAPI]);

  // This defines how workspace datasets should be maintained.
  // We automatically refresh the workspace datasets every 30 seconds.
  // Modifying datasets is a singular operation because other datasets are not modified in the process.
  const datasetsRefresh = 30 * 1000;
  const datasetsFetcher: DataCRUD<WorkspaceDataset> = useMemo(
    () => ({
      fetch: () => workspaceAPI.getWorkspaceDatasets(id),
      add: (dataset: WorkspaceDataset) =>
        workspaceAPI.addCompleteWorkspaceDataset(id, dataset),
      remove: (dataset: WorkspaceDataset) =>
        workspaceAPI.removeWorkspaceDataset(id, dataset.id),
      update: (dataset: WorkspaceDataset) =>
        workspaceAPI.updateWorkspaceDataset(id, dataset),
    }),
    [workspaceAPI, id]
  );
  const [datasets, datasetsCRUD] = useCRUD(
    datasetsFetcher,
    "id",
    false,
    datasetsRefresh
  );
  const datasetsContext = useMemo(
    () => ({ value: datasets, CRUD: datasetsCRUD }),
    [datasets, datasetsCRUD]
  );

  return (
    <WorkspaceContext.Provider
      value={{ workspace: workspace, datasets: datasetsContext }}
    >
      {children}
    </WorkspaceContext.Provider>
  );
};

export default WorkspaceWrapper;
export type { WorkspaceWrapperProps };
