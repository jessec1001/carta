import { FunctionComponent, useMemo } from "react";
import { DataCRUD, useAPI, useCRUD } from "hooks";
import { WorkspaceDataset } from "library/api";
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
    <WorkspaceContext.Provider value={{ datasets: datasetsContext }}>
      {children}
    </WorkspaceContext.Provider>
  );
};

export default WorkspaceWrapper;
export type { WorkspaceWrapperProps };
