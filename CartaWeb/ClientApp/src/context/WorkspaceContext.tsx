import { createContext } from "react";
import { DataCRUD, DataValue } from "hooks";
import { WorkspaceDataset } from "library/api";

/** Represents CRUD operations and a collection of data for a particular data type. */
interface DataCRUDWithValue<TData> {
  /** The CRUD operations for the data type. */
  CRUD: DataCRUD<TData>;
  /** The collection of data. */
  value: DataValue<TData>;
}

/** The type of value of {@link WorkspaceContext}. */
interface WorkspaceContextValue {
  datasets: DataCRUDWithValue<WorkspaceDataset>;
}

/** A context to provide easy access to retrieving and managing data related to a workspace. */
const WorkspaceContext = createContext<WorkspaceContextValue>(undefined!);

export default WorkspaceContext;
export type { WorkspaceContextValue };
