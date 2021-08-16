import { FunctionComponent, useEffect, useMemo, useState } from "react";
import { DataCRUD, useAPI, useCRUD } from "hooks";
import { Workspace, WorkspaceDataset, WorkspaceWorkflow } from "library/api";
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
  const { workspaceAPI, workflowAPI } = useAPI();

  // We store the original workspace object itself.
  // TODO: Some hook that combines useMounted and useEffect for async loading functions. Perhaps periodic as well.
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

  // This defines how workspace workflows should be maintained.
  // We automatically refresh the workspace workflows every 30 seconds.
  // Modifying workflows is a singular operation because other workflows are not affected.
  const workflowsRefresh = 30 * 1000;
  const workflowsFetcher: DataCRUD<WorkspaceWorkflow> = useMemo(
    () => ({
      fetch: () => workspaceAPI.getWorkspaceWorkflows(id),
      add: async (workflow: WorkspaceWorkflow) => {
        const workflowNew = await workflowAPI.createWorkflow(workflow);
        await workflowAPI.commitWorkflowVersion(
          workflowNew.id,
          "Initial verison"
        );
        const workspaceWorkflow = await workspaceAPI.addWorkspaceWorkflow(
          id,
          workflowNew.id
        );
        return workspaceWorkflow;
      },
      remove: (workflow: WorkspaceWorkflow) =>
        workspaceAPI.removeWorkspaceWorkflow(id, workflow.id),
      // TODO: We use the GET method here for updates because updates are actually in reference to versions
      // which are a subcollection of objects.
      update: (workflow: WorkspaceWorkflow) =>
        workspaceAPI.getWorkspaceWorkflow(id, workflow.id),
    }),
    [workspaceAPI, workflowAPI, id]
  );
  const [workflows, workflowsCRUD] = useCRUD(
    workflowsFetcher,
    "id",
    false,
    workflowsRefresh
  );
  const workflowsContext = useMemo(
    () => ({ value: workflows, CRUD: workflowsCRUD }),
    [workflows, workflowsCRUD]
  );

  return (
    <WorkspaceContext.Provider
      value={{
        workspace: workspace,
        datasets: datasetsContext,
        workflows: workflowsContext,
      }}
    >
      {children}
    </WorkspaceContext.Provider>
  );
};

export default WorkspaceWrapper;
export type { WorkspaceWrapperProps };
