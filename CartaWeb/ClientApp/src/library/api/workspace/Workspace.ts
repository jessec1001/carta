import { Modify } from "types";
import { Operation } from "../operations";
import { WorkspaceOperation } from "../workspace";
import { Document, DocumentDTO, Identifiable, parseDocument } from "../base";
import { WorkspaceChange } from "./WorkspaceChange";
import { WorkspaceUser } from "./WorkspaceUser";

/** Represents a workspace object. */
interface Workspace extends Document, Identifiable {
  /** Whether the workspace has been archived or not. */
  archived: boolean;

  /** The users that have been given access to the workspace. */
  users?: WorkspaceUser[];
  /** The operations that are contained in a workspace. */
  operations?: (WorkspaceOperation | (WorkspaceOperation & Operation))[];
  /** The changes that have been made to the workspace. */
  changes?: WorkspaceChange[];
}
/** Represents a workspace object as returned by the API server. */
type WorkspaceDTO = Modify<Workspace, DocumentDTO>;

/**
 * Converts a workspace data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspace = (dto: WorkspaceDTO): Workspace => {
  const document = parseDocument(dto);

  return {
    ...dto,
    ...document,
  };
};

export { parseWorkspace };
export type { Workspace, WorkspaceDTO };
