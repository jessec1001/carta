import { FunctionComponent, useContext } from "react";
import { UserContext } from "context";
import {
  defaultWorkspaceDatasetName,
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
/**
 * Determines the change type of an update to a workspace dataset. This is needed because the API does not currently
 * provide enough information to determine the change type per entry.
 * @param changes The complete change history of the workspace (or at least the subset of dataset changes).
 * @param index The index of the current change.
 * @returns The type of dataset update that was applied.
 */
const resolveDatasetChangeType = (
  changes: WorkspaceChange[],
  index: number
):
  | "renamed"
  | "workflowVersion"
  | "workflowAssign"
  | "workflowUnassign"
  | null => {
  // Find the index of the last update to the same dataset.
  let previousIndex = index;
  do previousIndex++;
  while (
    previousIndex < changes.length &&
    changes[previousIndex].id !== changes[index].id
  );

  // If no such entry exists, something is wrong and we should indicate it.
  if (previousIndex >= changes.length) return null;
  const currentChangeInfo = changes[index].workspaceChangeInformation;
  const previousChangeInfo = changes[previousIndex].workspaceChangeInformation;
  if (currentChangeInfo === undefined || previousChangeInfo === undefined)
    return null;

  // If the workflow identifier was changed, the workflow was reassigned completely.
  // If there is no workflow assigned anymore, the previous workflow was unassigned.
  // Else, a new workflow was assigned on top of an old workflow.
  if (currentChangeInfo.workflowId !== previousChangeInfo.workflowId) {
    if (currentChangeInfo.workflowId) return "workflowAssign";
    else return "workflowUnassign";
  }

  // If the workflow version was changed, then this indicates only the version was changed.
  if (currentChangeInfo.workflowVersion !== previousChangeInfo.workflowVersion)
    return "workflowVersion";

  // If none of the workflow information has changed, then, the dataset was renamed.
  return "renamed";
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
  let performedAction: string = "[UNKNOWN]";
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

    case WorkspaceChangeType.Dataset:
      const targetDatasetSource =
        change.workspaceChangeInformation!.datasetSource!;
      const targetDatasetResource =
        change.workspaceChangeInformation!.datasetResource!;
      const targetDatasetName = change.name
        ? `"${change.name}"`
        : defaultWorkspaceDatasetName({
            source: targetDatasetSource,
            resource: targetDatasetResource,
          });
      switch (change.workspaceAction.type) {
        case WorkspaceActionType.Added:
          performedAction = `added the ${targetDatasetName} dataset to the workspace.`;
          break;
        case WorkspaceActionType.Removed:
          performedAction = `removed the ${targetDatasetName} dataset from the workspace.`;
          break;
        case WorkspaceActionType.Updated:
          const datasetUpdateType = resolveDatasetChangeType(changes, index);

          const workflowName = change.workspaceChangeInformation?.workflowName;
          const workflowVersion =
            change.workspaceChangeInformation?.workflowVersion;
          const workflowVersionSuffix =
            workflowVersion === undefined ? "" : `(v${workflowVersion})`;

          switch (datasetUpdateType) {
            case "renamed":
              performedAction = `renamed the ${targetDatasetName} dataset.`;
              break;
            case "workflowAssign":
              performedAction = `assigned the "${workflowName}" ${workflowVersionSuffix} workflow on the ${targetDatasetName} dataset.`;
              break;
            case "workflowUnassign":
              performedAction = `unassigned the workflow on the ${targetDatasetName} dataset.`;
              break;
            case "workflowVersion":
              performedAction = `changed the version of the "${workflowName}" workflow to ${workflowVersionSuffix} on the ${targetDatasetName} dataset.`;
              break;
          }
          break;
      }
      break;

    case WorkspaceChangeType.Workflow:
      const workflowVersion =
        change.workspaceChangeInformation?.workflowVersion;
      const workflowVersionSuffix =
        workflowVersion === undefined ? "" : `(v${workflowVersion})`;
      switch (change.workspaceAction.type) {
        case WorkspaceActionType.Added:
          performedAction = `added the "${change.name}" ${workflowVersionSuffix} workflow to the workspace.`;
          break;
        case WorkspaceActionType.Removed:
          performedAction = `removed the "${change.name}" ${workflowVersionSuffix} workflow from the workspace.`;
          break;
        case WorkspaceActionType.Updated:
          performedAction = `changed the "${change.name}" workflow to ${workflowVersionSuffix}.`;
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
