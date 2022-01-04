/** The simple types that a JSON schema can specify. */
type JsonSchemaBasicTypename =
  | "file"
  | "object"
  | "array"
  | "string"
  | "integer"
  | "number"
  | "boolean"
  | "null"
  | undefined;

/** Represents the base for any schema type. */
interface JsonBaseSchema {
  $ref?: string;
  oneOf?: JsonBaseSchema[];
  allOf?: JsonBaseSchema[];

  type?: JsonSchemaBasicTypename | JsonSchemaBasicTypename[];
  title?: string;
  description?: string;
  default?: any;
  examples?: any[];

  ["ui:widget"]?: string;
}
/** Represents the typed base for specific schema types. */
interface JsonBaseTypedSchema<
  SchemaType extends JsonSchemaBasicTypename,
  ValueType
> extends JsonBaseSchema {
  type: SchemaType;
  default?: ValueType;
  examples?: ValueType[];
}

const anySchema: JsonBaseSchema = {
  type: [
    "file",
    "object",
    "array",
    "string",
    "integer",
    "number",
    "boolean",
    "null",
  ],
};
const neverSchema: JsonBaseSchema = {
  type: [],
};

const isContainer = <T extends JsonBaseSchema>(schema: T) => {
  return schema.type === "array" || schema.type === "object";
};

// Export schema types.
export type { JsonSchemaBasicTypename };
export type { JsonBaseSchema, JsonBaseTypedSchema };
export { anySchema, neverSchema, isContainer };
