import { Modify } from "types";
import { Document, DocumentDTO, Identifiable, parseDocument } from "../base";
import { WorkflowVersion } from "../workflow";

/** Represents a workspace workflow object. */
interface WorkspaceWorkflow extends Document, Identifiable {
  /** The version information about the workflow. */
  versionInformation: WorkflowVersion;

  /** Whether the workflow is archived within the workspace. */
  archived: boolean;
}
/** Represents a workspace workflow object as returned by the API server. */
type WorkspaceWorkflowDTO = Modify<WorkspaceWorkflow, DocumentDTO>;

/**
 * Converts a workspace workflow data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkspaceWorkflow = (
  dto: WorkspaceWorkflowDTO
): WorkspaceWorkflow => {
  const document = parseDocument(dto);

  return {
    ...dto,
    ...document,
  };
};

export { parseWorkspaceWorkflow };
export type { WorkspaceWorkflow, WorkspaceWorkflowDTO };
