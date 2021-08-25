import { Modify } from "types";
import { Document, DocumentDTO, Identifiable, parseDocument } from "../base";

/** Represents a workspace dataset object. */
interface WorkspaceDataset extends Identifiable, Document {
  /** The dataset source. */
  source: string;
  /** The dataset resource. */
  resource: string;

  /** The workflow applied to this dataset. */
  workflow?: string;
  /** The version of the workflow applied to this dataset. */
  workflowVersion?: number;
}
/** Represents a workspace dataset object as returned by the API server. */
type WorkspaceDatasetDTO = Modify<
  WorkspaceDataset,
  DocumentDTO & {
    workflow?: never;
    workflowId?: string;
  }
>;

/**
 * Constructs the default name for a workspace dataset from its source and resource.
 * @param dataset The workspace dataset to use.
 * @returns The default name.
 */
const defaultWorkspaceDatasetName = <
  TWorkspace extends { source: string; resource: string }
>({
  source,
  resource,
}: TWorkspace) => {
  return `(${source}/${resource})`;
};
/**
 * Converts a workspace dataset data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @param defaultName Whether to add a default name to the dataset if it does not exist. Defaults to `true`.
 * @returns The converted literal object.
 */
const parseWorkspaceDataset = (
  dto: WorkspaceDatasetDTO,
  defaultName: boolean = true
): WorkspaceDataset => {
  const { name, source, resource, workflowId, ...rest } = dto;
  const document = parseDocument(rest);

  return {
    ...rest,
    ...document,
    name: defaultName
      ? name ?? defaultWorkspaceDatasetName({ source, resource })
      : name,
    source: source,
    resource: resource,
    workflow: workflowId,
  };
};

export { defaultWorkspaceDatasetName, parseWorkspaceDataset };
export type { WorkspaceDataset, WorkspaceDatasetDTO };
