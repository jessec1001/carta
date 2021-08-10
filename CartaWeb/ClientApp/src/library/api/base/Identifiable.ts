/** Represents an object that is generally identifiable. */
interface Identifiable {
  /** The unique identifier of the object. */
  id: string;
  /** The user-friendly name of the object. */
  name?: string;
}

export type { Identifiable };
