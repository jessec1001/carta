import { Modify } from "types";

/** Represents a workspace dataset object. */
interface WorkspaceDataset {
  /** The unique identifier of the workspace dataset. */
  id: string;
  /** The user-friendly name of the workspace. */
  name?: string;
  /** The dataset source. */
  source: string;
  /** The dataset resource. */
  resource: string;
  /** The workflow applied to this dataset. */
  workflowId?: string;

  /** The date that the dataset was added to the workspace. */
  dateAdded?: Date;

  /** Whom the dataset was added to the workspace by. */
  addedBy?: string;
}
/** Represents a workspace dataset object as returned by the API server. */
type WorkspaceDatasetDTO = Modify<
  WorkspaceDataset,
  {
    dateAdded?: string;
    dateDeleted?: string;
  }
>;

/**
 * Converts a workspace dataset data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspaceDataset = (dto: WorkspaceDatasetDTO): WorkspaceDataset => {
  const { dateAdded, ...rest } = dto;

  return {
    ...rest,
    dateAdded: dateAdded ? new Date(dateAdded) : undefined,
  };
};

export { parseWorkspaceDataset };
export type { WorkspaceDataset, WorkspaceDatasetDTO };
