import { OperationSchema } from ".";
import { Identifiable } from "../base";

interface Operation extends Identifiable {
  type: string;
  subtype: string | null;

  default?: Record<string, any>;
  schema?: OperationSchema;
}

export default Operation;
