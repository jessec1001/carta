import { Modify } from "types";
import { Document, DocumentDTO, Identifiable, parseDocument } from "../base";

/** Represents a workspace user object. */
interface WorkspaceUser extends Document {
  /** The identifying information about the user. */
  userInformation: Identifiable;
}
/** Represents a workspace user object as returned by the API server. */
type WorkspaceUserDTO = Modify<WorkspaceUser, DocumentDTO>;

/**
 * Converts a workspace user data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspaceUser = (dto: WorkspaceUserDTO): WorkspaceUser => {
  const document = parseDocument(dto);

  return {
    ...dto,
    ...document,
  };
};

/**
 * Checks if a workspace user is currently active in a workspace.
 * @param user The workspace user.
 * @returns `true` if the workspace user is active; otherwise, `false`.`
 */
const isWorkspaceUserActive = (user: WorkspaceUser): boolean => {
  const documentHistory = user.documentHistory;
  if (!documentHistory) return true;

  const { dateAdded, dateDeleted } = documentHistory;

  if (dateAdded === undefined) return false;
  if (dateDeleted === undefined) return true;
  return dateAdded > dateDeleted;
};

export { parseWorkspaceUser, isWorkspaceUserActive };
export type { WorkspaceUser, WorkspaceUserDTO };
