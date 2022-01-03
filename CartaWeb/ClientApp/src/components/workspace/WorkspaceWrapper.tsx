import { FunctionComponent, useEffect, useState } from "react";
import { useAPI } from "hooks";
import { Workspace } from "library/api";
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

  return (
    <WorkspaceContext.Provider
      value={{
        workspace: workspace,
      }}
    >
      {children}
    </WorkspaceContext.Provider>
  );
};

export default WorkspaceWrapper;
export type { WorkspaceWrapperProps };
