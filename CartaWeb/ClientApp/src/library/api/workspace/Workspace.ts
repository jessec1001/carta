import { Modify } from "types";
import { WorkspaceDataset } from "./WorkspaceDataset";
import { WorkspaceUser } from "./WorkspaceUser";

/** Represents a workspace object. */
interface Workspace {
  /** The unique identifier of the workspace. */
  id: string;
  /** The user-friendly name of the workspace. */
  name: string;

  /** Whether the workspace has been archived or not. */
  archived: boolean;

  /** The date that the workspace was created on. */
  dateCreated?: Date;
  /** The date that the workspace was archived on. */
  dateArchived?: Date;
  /** The date that the workspace was unarchived on. */
  dateUnarchived?: Date;

  /** Whom the workspace was created by. */
  createdBy?: string;

  /** The users that have been given access to the workspace. */
  users?: WorkspaceUser[];

  /** The datasets contained in the workspace. */
  datasets?: WorkspaceDataset[];
}
/** Represents a workspace object as returned by the API server. */
type WorkspaceDTO = Modify<
  Workspace,
  {
    dateCreated?: string;
    dateArchived?: string;
    dateUnarchived?: string;
  }
>;

/**
 * Converts a workspace data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspace = (dto: WorkspaceDTO): Workspace => {
  const { dateCreated, dateArchived, dateUnarchived, ...rest } = dto;

  return {
    ...rest,
    dateCreated: dateCreated ? new Date(dateCreated) : undefined,
    dateArchived: dateArchived ? new Date(dateArchived) : undefined,
    dateUnarchived: dateUnarchived ? new Date(dateUnarchived) : undefined,
  };
};

export { parseWorkspace };
export type { Workspace, WorkspaceDTO };
