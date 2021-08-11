import { Modify } from "types";
import { Document, DocumentDTO, Identifiable, parseDocument } from "../base";

/** Represents a workspace dataset object. */
interface WorkspaceDataset extends Identifiable, Document {
  /** The dataset source. */
  source: string;
  /** The dataset resource. */
  resource: string;

  /** The workflow applied to this dataset. */
  workflow?: string;
  /** The version of the workflow applied to this dataset. */
  workflowVersion?: number;
}
/** Represents a workspace dataset object as returned by the API server. */
type WorkspaceDatasetDTO = Modify<
  WorkspaceDataset,
  DocumentDTO & {
    workflow?: never;
    workflowId?: string;
  }
>;

/**
 * Converts a workspace dataset data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspaceDataset = (dto: WorkspaceDatasetDTO): WorkspaceDataset => {
  const { workflowId, ...rest } = dto;
  const document = parseDocument(rest);

  return {
    ...rest,
    ...document,
    workflow: workflowId,
  };
};

export { parseWorkspaceDataset };
export type { WorkspaceDataset, WorkspaceDatasetDTO };
