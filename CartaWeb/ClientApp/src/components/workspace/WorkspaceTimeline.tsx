import { FunctionComponent, useContext } from "react";
import { UserContext } from "components/user";
import {
  User,
  WorkspaceActionType,
  WorkspaceChange,
  WorkspaceChangeType,
} from "library/api";

import "./WorkspaceTimeline.css";

/**
 * Resolves the display name for a specific user. If the username matches the current user, "You" is substituted for
 * "@username".
 * @param user The current user.
 * @param name The name to resolve.
 * @returns The display name.
 */
const resolveUserName = (user: User | null, name?: string): string => {
  if (name === user?.name) return "You";
  else return `@${name}`;
};

/** The props used for the {@link WorkspaceChangeItem} component. */
interface WorkspaceChangeItemProps {
  /** The list of all workspace changes. */
  changes: WorkspaceChange[];
  /** The index of the change to render. */
  index: number;
}

/** A component that produces some descriptive text about a change applied to a workspace. */
const WorkspaceChangeItem: FunctionComponent<WorkspaceChangeItemProps> = ({
  changes,
  index,
}) => {
  // We use the currently authenticated user to check against the usernames of users who made changes.
  const { user } = useContext(UserContext);

  // Retrieves the name of the user who performed the change.
  const change = changes[index];
  const performingUserName = change.workspaceAction.userName;

  // Computes a string representing the change applied to a workspace.
  let performedAction = "[UNKNOWN]";
  switch (change.changeType) {
    case WorkspaceChangeType.Workspace:
      switch (change.workspaceAction.type) {
        case WorkspaceActionType.Added:
          performedAction = `created the "${change.name}" workspace.`;
          break;
      }
      break;

    case WorkspaceChangeType.User:
      const targetUserName = resolveUserName(user, change.name);
      switch (change.workspaceAction.type) {
        case WorkspaceActionType.Added:
          performedAction = `added ${targetUserName} to the workspace.`;
          break;
        case WorkspaceActionType.Removed:
          performedAction = `removed ${targetUserName} from the workspace.`;
          break;
      }
      break;

    case WorkspaceChangeType.Operation:
      switch (change.workspaceAction.type) {
        case WorkspaceActionType.Added:
          performedAction = change.name
            ? `added the "${change.name}" operation to the workspace.`
            : `added an operation to the workspace.`;
          break;
        case WorkspaceActionType.Removed:
          performedAction = change.name
            ? `removed the "${change.name}" operation from the workspace.`
            : `removed an operation from the workspace.`;
          break;
      }
      break;
  }

  return (
    <span>
      {resolveUserName(user, performingUserName)} {performedAction}
    </span>
  );
};

/** The props used for the {@link WorkspaceTimeline} component. */
interface WorkspaceTimelineProps {
  /** The list of workspace changes. The changes are assumed to be sorted from newest to oldest. */
  changes: WorkspaceChange[];

  /** An optional starting index (inclusive) for rendering the changes. Defaults to 0. */
  indexStart?: number;
  /** An optional ending index (exclusive) for rending the changes. Defaults to the length of the changes. */
  indexEnd?: number;
}

/** A component that renders a list of changes to a workspace as  */
const WorkspaceTimeline: FunctionComponent<WorkspaceTimelineProps> = ({
  changes,
  indexStart,
  indexEnd,
}) => {
  // Compute the defaults for the start and end indices.
  indexStart = indexStart ?? 0;
  indexEnd = indexEnd ?? changes.length;

  // Get the list of changes that we are to render and whether to leave trails.
  const limitedChanges = changes.slice(indexStart, indexEnd);
  const trailingStart = indexStart > 0;
  const trailingEnd = indexEnd < changes.length;

  return (
    <ol className={"WorkspaceTimeline"}>
      {trailingStart && <div className={"WorkspaceTimeline-Trail start"} />}
      {limitedChanges.map((change, index) => (
        <li key={index} className="WorkspaceTimeline-Item">
          <div className={"WorkspaceTimeline-Trail"} />
          <WorkspaceChangeItem changes={changes} index={index} />
        </li>
      ))}
      {trailingEnd && <div className={"WorkspaceTimeline-Trail end"} />}
    </ol>
  );
};

export default WorkspaceTimeline;
export type { WorkspaceTimelineProps };
