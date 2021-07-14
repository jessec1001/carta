import { JsonBaseSchema, neverSchema } from "./JsonBaseSchema";
import { JsonSchema } from "./JsonSchema";
import { JsonArraySchema, JsonObjectSchema } from "./types";

/**
 * Trims the starting and ending slashes from a string.
 * @param str The string to trim.
 * @returns The trimmed string.
 */
const trimSlashes = (str: string) => {
  if (str.startsWith("/")) str = str.substring(1);
  if (str.endsWith("/")) str = str.substring(0, str.length - 1);
  return str;
};
/**
 * Resolves a reference to a schema definition.
 * @param schema The overall schema which contains definitions.
 * @param reference The reference path to another schema.
 * @returns The resolved schema.
 */
const resolveDefinition = (schema: JsonSchema, reference: string) => {
  // We trim slashes to make path handling easier.
  reference = trimSlashes(reference);

  // We will only support internal references to the definitions list.
  if (reference.startsWith("definitions")) {
    if (schema.definitions === undefined) return neverSchema;
    else {
      reference = trimSlashes(reference.replace("definitions", ""));
      return schema.definitions[reference];
    }
  } else return neverSchema;
};

/**
 * Flattens a specified object schema so it is simpler and easier to understand.
 * @param schema The overall schema.
 * @param subschema The particular object subschema to flatten.
 * @returns The flattened subschema.
 */
const flattenSchemaObject = (
  schema: JsonSchema,
  subschema: JsonObjectSchema
) => {
  // Copy the schema so we do not accidentally mutate the object.
  const flatSchema: JsonObjectSchema = { ...subschema };

  // Recursively flatten all of the property schemas.
  if (typeof flatSchema.properties === "object") {
    const flatProperties: Record<string, JsonBaseSchema> = {};
    for (const property in flatSchema.properties) {
      flatProperties[property] = flattenSchemaBase(
        schema,
        flatSchema.properties[property]
      );
    }
    flatSchema.properties = flatProperties;
  }

  // Flatten the additional properties schema.
  if (typeof flatSchema.additionalProperties === "object") {
    flatSchema.additionalProperties = flattenSchemaBase(
      schema,
      flatSchema.additionalProperties
    );
  }

  return flatSchema;
};
/**
 * Flattens a specified array schema so it is simpler and easier to understand.
 * @param schema The overall schema.
 * @param subschema The particular array subschema to flatten.
 * @returns The flattened subschema.
 */
const flattenSchemaArray = (schema: JsonSchema, subschema: JsonArraySchema) => {
  // Copy the schema so we do not accidentally mutate the object.
  const flatSchema: JsonArraySchema = { ...subschema };

  // Recursively flatten all of the item schemas.
  if (flatSchema.items) {
    if (Array.isArray(flatSchema.items)) {
      const flatItems: JsonBaseSchema[] = [];
      for (let k = 0; k < flatSchema.items.length; k++)
        flatItems[k] = flattenSchemaBase(schema, flatSchema.items[k]);
      flatSchema.items = flatItems;
    } else {
      const flatItem = flattenSchemaBase(schema, flatSchema.items);
      flatSchema.items = flatItem;
    }
  }

  // Flatten the additional items schema.
  if (typeof flatSchema.additionalItems === "object") {
    flatSchema.additionalItems = flattenSchemaBase(
      schema,
      flatSchema.additionalItems
    );
  }

  return flatSchema;
};
/**
 * Flattens a specified base schema so it is simpler and easier to understand.
 * @param schema The overall schema.
 * @param subschema The particular base subschema to flatten.
 * @returns The flattened subschema.
 */
const flattenSchemaBase = (
  schema: JsonSchema,
  subschema: JsonBaseSchema
): JsonBaseSchema => {
  // If we have a reference to a definition, we replace that reference with the corresponding definition schema.
  if (subschema.$ref) {
    const ref = subschema.$ref;
    // Notice that we only perform same-schema resolutions so far.
    if (ref.startsWith("#")) {
      return resolveDefinition(schema, ref.replace("#", ""));
    }
  }

  // We can flatten one-of and all-of schemas if they only contain one element.
  // This avoid the schema from becoming overly complex.
  if (subschema.oneOf) {
    const { oneOf, ...subschemaRest } = subschema;
    if (oneOf.length === 1)
      return { ...flattenSchemaBase(schema, oneOf[0]), ...subschemaRest };
  }
  if (subschema.allOf) {
    const { allOf, ...subschemaRest } = subschema;
    if (allOf.length === 1)
      return { ...flattenSchemaBase(schema, allOf[0]), ...subschemaRest };
  }

  // If we are working with an object or property, we need to flatten their subschemas.
  if (Array.isArray(subschema.type)) {
    if (subschema.type.includes("object"))
      return flattenSchemaObject(schema, subschema as JsonObjectSchema);
    if (subschema.type.includes("array"))
      return flattenSchemaArray(schema, subschema as JsonArraySchema);
  } else {
    if (subschema.type === "object")
      return flattenSchemaObject(schema, subschema as JsonObjectSchema);
    if (subschema.type === "array")
      return flattenSchemaArray(schema, subschema as JsonArraySchema);
  }

  // By default we simply return the original schema if it is already flat.
  return subschema;
};

/**
 * Flattens a specified schema so it is simpler and easier to understand.
 * @param schema The schema to flatten.
 * @returns The flattened schema.
 */
const flattenSchema = <T extends JsonSchema>(schema: T) => {
  return flattenSchemaBase(schema, schema);
};

export default flattenSchema;
export { flattenSchemaBase, flattenSchemaArray, flattenSchemaObject };
