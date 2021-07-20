import { Modify } from "types";

/** Represents a workspace user object. */
interface WorkspaceUser {
  /** The unique identifier of the workspace user. */
  id: string;
  /** The username of the workspace user. */
  name: string;

  /** The date that the user was added to the workspace. */
  dateAdded?: Date;
  /** The date that the user was delete from the workspace. */
  dateDeleted?: Date;

  /** Whom the user was added to the workspace by. */
  addedBy?: string;
  /** Whom the user was deleted from the workspace by. */
  deletedBy?: string;
}
/** Represents a workspace user object as returned by the API server. */
type WorkspaceUserDTO = Modify<
  WorkspaceUser,
  {
    dateAdded?: string;
    dateDeleted?: string;
  }
>;

/**
 * Converts a workspace user data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspaceUser = (dto: WorkspaceUserDTO): WorkspaceUser => {
  const { dateAdded, dateDeleted, ...rest } = dto;

  return {
    ...rest,
    dateAdded: dateAdded ? new Date(dateAdded) : undefined,
    dateDeleted: dateDeleted ? new Date(dateDeleted) : undefined,
  };
};

/**
 * Checks if a workspace user is currently active in a workspace.
 * @param user The workspace user.
 * @returns `true` if the workspace user is active; otherwise, `false`.`
 */
const isWorkspaceUserActive = (user: WorkspaceUser): boolean => {
  const { dateAdded, dateDeleted } = user;

  if (dateAdded === undefined) return false;
  if (dateDeleted === undefined) return true;
  return dateAdded > dateDeleted;
};

export { parseWorkspaceUser, isWorkspaceUserActive };
export type { WorkspaceUser, WorkspaceUserDTO };
