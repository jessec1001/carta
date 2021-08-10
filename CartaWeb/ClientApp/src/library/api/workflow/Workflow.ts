import { Modify } from "types";
import { Document, Identifiable, parseDocument } from "../base";
import { WorkflowOperation } from "./WorkflowOperation";

/** Represents a workflow object. */
interface Workflow extends Identifiable, Document {
  /** The current version number of the workflow. */
  versionNumber: number;

  /** The operations list that composes the workflow. */
  operations?: WorkflowOperation[];
}
/** Represents a workflow object as returned by the API server. */
type WorkflowDTO = Modify<Workflow, {}>;

/**
 * Converts a workflow data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseWorkflow = (dto: WorkflowDTO): Workflow => {
  const document = parseDocument(dto);
  const { ...rest } = dto;

  return {
    ...rest,
    ...document,
  };
};

export { parseWorkflow };
export type { Workflow, WorkflowDTO };
