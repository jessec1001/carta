import { Modify } from "types";

/** Represents the possible data type that a workspace change can fall apply to. */
enum WorkspaceChangeType {
  Workspace = "Workspace",
  User = "User",
  Operation = "Operation",
}
/** Represents the possible change action type that can be applied to a data type. */
enum WorkspaceActionType {
  Added = "Added",
  Removed = "Removed",
  Updated = "Updated",
}

/** Represents a change to a workspace. */
interface WorkspaceChange {
  /** The unique identifier of the workspace change. */
  id: string;
  /** The user-friendly name of the object that is the subject of the change. */
  name?: string;

  /** The type of data structure the change was applied to. */
  changeType: WorkspaceChangeType;

  /** Information about who performed what change. */
  workspaceAction: {
    /** The type of change that was performed. */
    type: WorkspaceActionType;

    /** The username of the user who performed the change. */
    userName: string;
    /** The date and time at which the change occurred. */
    dateTime: Date;
  };
}
/** Represents a change to a workspace as returned by the API server. */
type WorkspaceChangeDTO = Modify<
  WorkspaceChange,
  {
    workspaceAction: {
      type: WorkspaceActionType;

      userName: string;
      dateTime: string;
    };
  }
>;

/**
 * Converts a workspace change data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspaceChange = (dto: WorkspaceChangeDTO): WorkspaceChange => {
  const { workspaceAction, ...rest } = dto;

  return {
    ...rest,
    workspaceAction: {
      ...workspaceAction,
      dateTime: new Date(workspaceAction.dateTime),
    },
  };
};

export { parseWorkspaceChange };
export { WorkspaceActionType, WorkspaceChangeType };
export type { WorkspaceChange, WorkspaceChangeDTO };
