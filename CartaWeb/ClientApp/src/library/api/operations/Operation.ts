import { JsonSchema } from "library/schema";
import { Identifiable } from "../base";

interface Operation extends Identifiable {
  type: string;
  subtype: string | null;

  default?: Record<string, any>;
  input?: JsonSchema;
  output?: JsonSchema;
}

export default Operation;
