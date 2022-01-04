import { Modify } from "types";
import { Document, DocumentDTO, Identifiable, parseDocument } from "../base";

/** Represents a workspace operation object. */
interface WorkspaceOperation extends Document, Identifiable {}
/** Represents a workspace operation object as returned by the API server. */
type WorkspaceOperationDTO = Modify<WorkspaceOperation, DocumentDTO>;

/**
 * Converts a workspace operation data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspaceOperation = (
  dto: WorkspaceOperationDTO
): WorkspaceOperation => {
  const document = parseDocument(dto);

  return {
    ...dto,
    ...document,
  };
};

export { parseWorkspaceOperation };
export type { WorkspaceOperation, WorkspaceOperationDTO };
