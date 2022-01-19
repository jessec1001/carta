import { FunctionComponent, useEffect, useMemo, useState } from "react";
import { DataCRUD, useAPI, useCRUD } from "hooks";
import { Workspace, Operation } from "library/api";
import WorkspaceContext from "./WorkspaceContext";

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
  const { operationsAPI, workspaceAPI } = useAPI();

  // We store the original workspace object itself.
  // TODO: Some hook that combines useMounted and useEffect for async loading functions. Perhaps periodic as well.
  const [workspace, setWorkspace] = useState<Workspace | null>(null);
  useEffect(() => {
    (async () => {
      const workspace = await workspaceAPI.getWorkspace(id);
      setWorkspace(workspace);

      // TODO: Is there a better place for this?
      document.title = `${workspace.name} - Carta`;
    })();
  }, [id, workspaceAPI]);

  // This defines how workspace operations should be maintained.
  // We automatically refresh the workspace operations every 30 seconds.
  // Modifying operations is a singular operation because other operations are not modified in the process.
  const operationsRefresh = 30 * 1000;
  const operationsFetcher: DataCRUD<Operation> = useMemo(
    () => ({
      fetch: async () => {
        const workspaceOperations = await workspaceAPI.getWorkspaceOperations(
          id
        );
        const operations: Operation[] = [];
        for (const workspaceOperation of workspaceOperations) {
          operations.push(
            await operationsAPI.getOperation(workspaceOperation.id)
          );
        }
        return operations;
      },
      add: async (operation: Operation) => {
        await workspaceAPI.addWorkspaceOperation(id, operation.id);
        return operation;
      },
      remove: async (operation: Operation) => {
        await workspaceAPI.removeWorkspaceOperation(id, operation.id);
      },
      update: (operation: Operation) => Promise.resolve(operation),
    }),
    [id, operationsAPI, workspaceAPI]
  );
  const [operations, operationsCRUD] = useCRUD(
    operationsFetcher,
    "id",
    false,
    operationsRefresh
  );
  const operationsContext = useMemo(
    () => ({ value: operations, CRUD: operationsCRUD }),
    [operations, operationsCRUD]
  );

  return (
    <WorkspaceContext.Provider
      value={{
        workspace: workspace,
        operations: operationsContext,
      }}
    >
      {children}
    </WorkspaceContext.Provider>
  );
};

export default WorkspaceWrapper;
export type { WorkspaceWrapperProps };
