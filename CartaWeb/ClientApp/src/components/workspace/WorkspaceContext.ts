import { createContext, useContext } from "react";
import { Workspace } from "library/api";

/** The type of value for the {@link WorkspaceContext} context. */
interface IWorkspaceContext {
  /** The currently opened workspace. */
  workspace: Workspace;
}
/** The type of value for the {@link useWorkspace} hook. */
interface IWorkspace {
  /** The currently opened workspace. */
  workspace: Workspace;
}

/** A context to provide easy access to retrieving and managing data related to a workspace. */
const WorkspaceContext = createContext<IWorkspaceContext | undefined>(
  undefined
);

/**
 * A hook to provide easy access to retrieving and managing data related to a workspace.
 * @returns The current workspace and related data if available.
 */
const useWorkspace = (): IWorkspace => {
  const context = useContext(WorkspaceContext);
  if (context === undefined) {
    throw new Error(
      "Workspace context must be used within a workspace component."
    );
  }
  return context;
};

export default WorkspaceContext;
export { useWorkspace };
export type { IWorkspaceContext, IWorkspace };
