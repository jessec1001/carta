import { JsonBaseSchema } from "./JsonBaseSchema";
import {
  JsonArraySchema,
  JsonObjectSchema,
  JsonNumberSchema,
  JsonStringSchema,
  JsonBooleanSchema,
  JsonNullSchema,
  JsonEnumSchema,
  JsonMultitypeSchema,
} from "./types";

/** Any basic JSON schema type. */
type JsonSchemaBasicType =
  | JsonObjectSchema
  | JsonArraySchema
  | JsonNumberSchema
  | JsonStringSchema
  | JsonBooleanSchema
  | JsonNullSchema
  | JsonEnumSchema
  | JsonMultitypeSchema;

/** The type for a JSON schema of any type. */
type JsonSchema = JsonBaseSchema & {
  $schema?: string;
  id?: string;
  definitions?: Record<string, JsonSchema>;
};

// Export the JSON schema type and related types.
export type { JsonSchema, JsonSchemaBasicType };
