import { FunctionComponent, HTMLAttributes, useState } from "react";
import queryString from "query-string";
import { useAPI, useStoredState } from "hooks";
import { Workspace } from "library/api";
import { Button, ButtonGroup, CloseButton } from "components/buttons";
import { Card } from "components/card";
import { Text } from "components/text";
import { Link } from "components/link";
import WorkspaceTimeline from "./WorkspaceTimeline";
import styles from "./WorkspaceCard.module.css";
import { Modal } from "components/modal";
import { useNotifications } from "components/notifications";
import { LogSeverity } from "library/logging";
import classNames from "classnames";

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
  const [workspaceHistory] = useStoredState<Record<string, number>>(
    {},
    "workspaceHistory"
  );
  const [deleteModalActive, setDeleteModalActive] = useState<boolean>(false);
  const [deleted, setDeleted] = useState<boolean>(false);

  // Compute a string for the last accessed date.
  // If the workspace has never been accessed (unlikely), defaults to "never".
  const accessTimestamp: number | undefined = workspaceHistory[workspace.id];
  const accessDate =
    accessTimestamp === undefined ? undefined : new Date(accessTimestamp);
  const accessDateString = accessDate
    ? `${accessDate.toLocaleDateString()} at ${accessDate.toLocaleTimeString()}`
    : "never";

  // Compute a string for the last modified date.
  // If the workspace has never been modified (should be impossible), defaults to "never".
  const modifyDate =
    workspace.changes &&
    workspace.changes.length > 0 &&
    workspace.changes[0].workspaceAction.dateTime;
  const modifyDateString = modifyDate
    ? `${modifyDate.toLocaleDateString()} at ${modifyDate.toLocaleTimeString()}`
    : "never";

  // Handle the deletion of the workspace.
  const { logger } = useNotifications();
  const { workspaceAPI } = useAPI();
  const handleDelete = async () => {
    // We delete the workspace.
    setDeleted(true);
    try {
      await workspaceAPI.deleteWorkspace(workspace.id);
    } catch (error: any) {
      logger.log({
        source: "Workspace Card",
        severity: LogSeverity.Error,
        title: "Workspace Deletion Error",
        message: error.message,
        data: error,
      });
    }
  };

  const workspaceLink = queryString.stringifyUrl({
    url: "/workspace",
    query: { id: workspace.id },
  });
  return (
    <Card
      {...props}
      className={classNames(styles.card, { [styles.deleted]: deleted })}
    >
      <Modal
        blur
        uninteractive
        active={deleteModalActive}
        onClose={() => setDeleteModalActive(false)}
        className={styles.modal}
      >
        <Text padding="bottom">
          Are you sure you want to delete the workspace "{workspace.name}"?
        </Text>
        <ButtonGroup stretch>
          <Button
            color="error"
            onClick={() => {
              handleDelete();
              setDeleteModalActive(false);
            }}
          >
            Delete
          </Button>
          <Button color="secondary" onClick={() => setDeleteModalActive(false)}>
            Cancel
          </Button>
        </ButtonGroup>
      </Modal>
      <Card.Header className={styles.cardHeader}>
        <Link to={workspaceLink} color="normal" className={styles.cardLabel}>
          <Text size="medium" align="middle">
            {workspace.name}
          </Text>
        </Link>
        <CloseButton
          className={styles.cardDelete}
          onClick={() => setDeleteModalActive(true)}
        />
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
          <time dateTime={modifyDateString}>{modifyDateString}</time>
        </Text>

        {/* Render the date last accessed at. */}
        <Text color="muted" size="small">
          Last accessed at{" "}
          <time dateTime={accessDateString}>{accessDateString}</time>
        </Text>
      </Card.Footer>
    </Card>
  );
};

export default WorkspaceCard;
export type { WorkspaceCardProps };
