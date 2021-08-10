import { Modify } from "types";
import { Identifiable } from "./Identifiable";

/** Represents a generic document item representing an object in Carta. */
interface Document {
  /** The history of most recent changes to the object. */
  documentHistory?: {
    /** The date that the document was added. */
    dateAdded?: Date;
    /** The date that the document was removed. */
    dateDeleted?: Date;
    /** The date that the document was archived. */
    dateArchived?: Date;
    /** The date that the document was unarchived. */
    dateUnarchived?: Date;

    /** Information about the user who added the document, */
    addedBy?: Identifiable;
    /** Information about the user who removed the document. */
    deletedBy?: Identifiable;
    /** Information about the user who archived the document. */
    archivedBy?: Identifiable;
    /** Information about the user who unarchived the document. */
    unarchivedBy?: Identifiable;
  };
}
/** Represents a generic document item as returned by the API server. */
type DocumentDTO = Modify<
  Document,
  {
    dateAdded?: string;
    dateDeleted?: string;
    dateArchived?: string;
    dateUnarchived?: string;
  }
>;

/**
 * Converts a document data transfer object into a more useable literal object.
 * @param dto The data transfer object.
 * @returns The converted literal object.
 */
const parseDocument = <TDTO extends DocumentDTO>(dto: TDTO): Document => {
  const { documentHistory } = dto;
  if (!documentHistory) return {};

  const { dateAdded, dateDeleted, dateArchived, dateUnarchived, ...rest } =
    documentHistory;
  return {
    documentHistory: {
      ...rest,
      dateAdded: dateAdded && new Date(dateAdded),
      dateDeleted: dateDeleted && new Date(dateDeleted),
      dateArchived: dateArchived && new Date(dateArchived),
      dateUnarchived: dateUnarchived && new Date(dateUnarchived),
    },
  };
};

export { parseDocument };
export type { Document, DocumentDTO };
