/** Represents the base for any schema type. */
interface JsonSchemaProperty<T extends string | undefined> {
  type: T;
  title?: string;
  description?: string;
  default?: any;
  examples?: any[];
}

/** The schema specification for a JSON object. */
interface JsonSchemaObject extends JsonSchemaProperty<"object"> {
  properties?: Record<string, JsonSchemaType>;
  additionalProperties?: boolean;
  required?: Array<string>;
  minProperties?: number;
  maxProperties?: number;
}
/** The schema specification for a JSON array. */
interface JsonSchemaArray extends JsonSchemaProperty<"array"> {
  items?: JsonSchemaType | JsonSchemaType[];
  additionalItems?: JsonSchemaType | boolean;
  minItems?: number;
  maxItems?: number;
  uniqueItems?: boolean;
}
/** The schema specification for a JSON number. */
interface JsonSchemaNumber extends JsonSchemaProperty<"integer" | "number"> {
  multipleOf?: number;
  minimum?: number;
  maximum?: number;
  exclusiveMinimum?: boolean;
  exclusiveMaximum?: boolean;
}
/** The schema specification for a JSON string. */
interface JsonSchemaString extends JsonSchemaProperty<"string"> {
  minLength?: number;
  maxLength?: number;
  pattern?: string;
}
/** The schema specification for a JSON boolean. */
interface JsonSchemaBoolean extends JsonSchemaProperty<"boolean"> {}
/** The schema specification for a JSON null value. */
interface JsonSchemaNull extends JsonSchemaProperty<"null"> {}
/** The schema specification for a JSON enumeration value. */
interface JsonSchemaEnum extends JsonSchemaProperty<"string" | undefined> {
  enum: any[];
}

/** The schema specification for any JSON type. */
type JsonSchemaType =
  | JsonSchemaObject
  | JsonSchemaArray
  | JsonSchemaNumber
  | JsonSchemaString
  | JsonSchemaBoolean
  | JsonSchemaNull
  | JsonSchemaEnum;
/** The type for a JSON schema of any type. */
type JsonSchema = JsonSchemaType & {
  $schema: string;
  $id: string;
};

// Export the JSON schema and JSON types.
export default JsonSchema;
export type {
  JsonSchemaType,
  JsonSchemaObject,
  JsonSchemaArray,
  JsonSchemaNumber,
  JsonSchemaString,
  JsonSchemaBoolean,
  JsonSchemaNull,
  JsonSchemaEnum,
};
