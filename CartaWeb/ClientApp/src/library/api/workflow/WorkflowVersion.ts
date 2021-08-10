import { Modify } from "types";

/** Represents a version of a workflow object. */
interface WorkflowVersion {
  /** The version number. */
  number: number;
  /** The version number that this version is based on. */
  baseNumber: number;

  /** The description for the workflow. */
  description: string;

  /** The date that this version was created. */
  dateCreated: Date;

  /** The user that this version was created by. */
  createdBy: {
    /** The unique identifier of the user. */
    id: string;
    /** The username of the user. */
    name: string;
  };
}
/** Represents a version of a workflow object as returned by the API server. */
type WorkflowVersionDTO = Modify<
  WorkflowVersion,
  {
    dateCreated: string;
  }
>;

/**
 * Converts a workflow version data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkflowVersion = (dto: WorkflowVersionDTO): WorkflowVersion => {
  const { dateCreated, ...rest } = dto;

  return {
    ...rest,
    dateCreated: new Date(dateCreated),
  };
};

export { parseWorkflowVersion };
export type { WorkflowVersion, WorkflowVersionDTO };
