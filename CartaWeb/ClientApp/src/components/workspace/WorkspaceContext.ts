import { createContext, useContext } from "react";
import { DataCRUD, DataValue } from "hooks";
import { Workspace, Operation } from "library/api";

/** Represents CRUD operations and a collection of data for a particular data type. */
interface DataCRUDWithValue<TData> {
  /** The CRUD operations for the data type. */
  CRUD: DataCRUD<TData>;
  /** The collection of data. */
  value: DataValue<TData>;
}

/** The type of value for the {@link WorkspaceContext} context. */
interface IWorkspaceContext {
  /** The currently opened workspace. */
  workspace: Workspace | null;

  /** The operations contained in the workspace. */
  operations: DataCRUDWithValue<Operation>;
}
/** The type of value for the {@link useWorkspace} hook. */
interface IWorkspace {
  /** The currently opened workspace. */
  workspace: Workspace | null;

  /** The operations contained in the workspace. */
  operations: DataCRUDWithValue<Operation>;
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
    throw new Error("'useWorkspace' must be used within a 'WorkspaceProvider'");
  }
  return context;
};

export default WorkspaceContext;
export { useWorkspace };
export type { IWorkspaceContext, IWorkspace };
