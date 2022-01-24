import { JsonSchema } from "library/schema";

interface OperationSchema {
  inputs: Record<string, JsonSchema>;
  outputs: Record<string, JsonSchema>;
}

export default OperationSchema;
