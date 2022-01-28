import { OperationSchema } from ".";
import { Identifiable } from "../base";

/** Represents an instantiated operation. */
interface Operation extends Identifiable {
  /** The type of operation. */
  type: string;
  /** The subtype of operation. This should only be specified when {@link type} is a container type. */
  subtype: string | null;

  /** The default input values for the operation. */
  default?: Record<string, any>;
  /** The input and output schema for the operation. */
  schema?: OperationSchema;
}

export default Operation;
