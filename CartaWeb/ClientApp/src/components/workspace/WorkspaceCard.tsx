import { FunctionComponent, HTMLAttributes } from "react";
import { useStoredState } from "hooks";
import { Workspace } from "library/api";
import { Card } from "components/card";
import { Text } from "components/text";
import WorkspaceTimeline from "./WorkspaceTimeline";

/** The props used for the {@link WorkspaceCard} component. */
interface WorkspaceCardProps {
  /** The workspace to display the details of. */
  workspace: Workspace;
}

/** A component that renders the details of a workspace in a card format. */
const WorkspaceCard: FunctionComponent<
  WorkspaceCardProps & HTMLAttributes<HTMLDivElement>
> = ({ workspace, children, ...props }) => {
  // We load workspace accessions to obtain the last date that this workspace was accessed.s
  const [workspaceAccessions] = useStoredState<Record<string, number>>(
    {},
    "workspaceAccessions"
  );

  // Compute a string for the last accessed date.
  // If the workspace has never been accessed (unlikely), defaults to "never".
  const accessTimestamp: number | undefined = workspaceAccessions[workspace.id];
  const accessDate =
    accessTimestamp === undefined ? undefined : new Date(accessTimestamp);
  const accessDateString = accessDate
    ? `${accessDate.toLocaleDateString()} at ${accessDate.toLocaleTimeString()}`
    : "never";

  // Compute a string for the last modified date.
  // If the workspace has never been modified (should be impossible), defaults to "never".
  const modifyDate =
    workspace.changes && workspace.changes[0].workspaceAction.dateTime;
  const modifyDateString = modifyDate
    ? `${modifyDate.toLocaleDateString()} at ${modifyDate.toLocaleTimeString()}`
    : "never";

  return (
    <Card {...props} style={{ width: "16rem", flexShrink: 0 }}>
      <Card.Header>
        <Text size="medium">{workspace.name}</Text>
      </Card.Header>
      <Card.Body>
        {/* Render a timeline section for the workspace changes. */}
        <Text size="normal">Changes</Text>
        <Text size="small">
          {/* We limit changes in the workspace card view to 5 entires. */}
          <WorkspaceTimeline changes={workspace.changes ?? []} indexEnd={5} />
        </Text>
      </Card.Body>
      <Card.Footer>
        {/* Render the date last modified at. */}
        <Text color="muted" size="small">
          Last modified at{" "}
          <time dateTime={modifyDate?.toISOString()}>{modifyDateString}</time>
        </Text>

        {/* Render the date last accessed at. */}
        <Text color="muted" size="small">
          Last accessed at{" "}
          <time dateTime={accessDate?.toISOString()}>{accessDateString}</time>
        </Text>
      </Card.Footer>
    </Card>
  );
};

export default WorkspaceCard;
export type { WorkspaceCardProps };
