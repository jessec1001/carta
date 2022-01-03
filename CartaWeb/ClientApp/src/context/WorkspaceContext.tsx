import { createContext } from "react";
import { DataCRUD, DataValue } from "hooks";
import { Workspace } from "library/api";

/** Represents CRUD operations and a collection of data for a particular data type. */
interface DataCRUDWithValue<TData> {
  /** The CRUD operations for the data type. */
  CRUD: DataCRUD<TData>;
  /** The collection of data. */
  value: DataValue<TData>;
}

/** THINGS STORED PER WORKSPACE
 * For each, loading state and error state.
 *
 * Datasets:
 * - Add/Create `dataset = datasets.add(dataset)`
 * - Update `dataset = datasets.update(dataset)`
 * - Find `[dataset, error] datasets.find(datasetId)`
 *
 * Workflows:
 * - Add/Create (workflow = workflows.add(workflow))
 *
 * NOTICE: Versions form a sort of subcollection of workflows.
 * - Commit Version (version = workflows.commit(workflow, description))
 */

/** The type of value of {@link WorkspaceContext}. */
interface WorkspaceContextValue {
  workspace: Workspace | null;
}

/** A context to provide easy access to retrieving and managing data related to a workspace. */
const WorkspaceContext = createContext<WorkspaceContextValue>(undefined!);

export default WorkspaceContext;
export type { WorkspaceContextValue };
