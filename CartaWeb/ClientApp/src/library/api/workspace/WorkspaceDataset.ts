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
  workflow?: string;
  workflowVersion?: number;

  documentHistory?: {
    dateAdded?: Date;

    addedBy?: {
      id: string;
      name: string;
    };
  };
}
/** Represents a workspace dataset object as returned by the API server. */
type WorkspaceDatasetDTO = Modify<
  WorkspaceDataset,
  {
    workflow: never;
    workflowId?: string;

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
  const { dateAdded, dateDeleted, workflowId, ...rest } = dto;

  return {
    ...rest,
    dateAdded: dateAdded ? new Date(dateAdded) : undefined,
    dateDeleted: dateDeleted ? new Date(dateDeleted) : undefined,
    workflow: workflowId,
  };
};

export { parseWorkspaceDataset };
export type { WorkspaceDataset, WorkspaceDatasetDTO };
