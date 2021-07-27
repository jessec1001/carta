import {
  anySchema,
  JsonBaseSchema,
  JsonBaseTypedSchema,
  neverSchema,
} from "../JsonBaseSchema";

/** The type of widgets that can be used to input a JSON object. */
enum JsonObjectSchemaWidgets {
  List = "list",
  User = "user",
}

/** The schema specification for a JSON object. */
interface JsonObjectSchema extends JsonBaseTypedSchema<"object", any> {
  properties?: Record<string, JsonBaseSchema>;
  additionalProperties?: JsonBaseSchema | boolean;
  required?: Array<string>;
  minProperties?: number;
  maxProperties?: number;

  "ui:widget"?: JsonObjectSchemaWidgets;
}

/**
 * Computes a subschema with a given key in an object schema.
 * @param schema The object schema.
 * @param property The property key of the object.
 */
const objectSubschema = (
  schema: JsonObjectSchema,
  property: string
): JsonBaseSchema => {
  // If the properties schema is defined, we check for subschemas there first.
  if (schema.properties !== undefined) {
    if (property in schema.properties) return schema.properties[property];
  }

  // If no previous conditions have been met, we try to use the additional properties schema.
  // If the addition properties schema is undefined, assume any other properties are allowed.
  if (schema.additionalProperties === undefined) return anySchema;
  // If the addition properties schema is a boolean, we allow more properties if true and no more properties otherwise.
  else if (typeof schema.additionalProperties === "boolean") {
    if (schema.additionalProperties) return anySchema;
    else return neverSchema;
  }
  // If the additional properties schema is an actual schema, use it.
  else return schema.additionalProperties;
};

export default JsonObjectSchema;
export { objectSubschema, JsonObjectSchemaWidgets };
