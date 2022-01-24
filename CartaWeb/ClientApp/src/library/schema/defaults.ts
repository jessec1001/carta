import { JsonSchema } from "./JsonSchema";
import { JsonBaseSchema, JsonSchemaBasicTypename } from "./JsonBaseSchema";
import {
  arraySubschema,
  objectSubschema,
  JsonArraySchema,
  JsonObjectSchema,
  JsonNumberSchema,
  JsonStringSchema,
  JsonBooleanSchema,
  JsonNullSchema,
  JsonEnumSchema,
} from "./types";

/**
 * Computes a default value for the specified object schema.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type and contain
 * the correct primitive types.
 * Note: this default value will satisfy any non-additional properties specified even if not marked as required.
 * @param schema The object schema to generate a default value for.
 * @param value The existing value to merge with the default value. This value should be an object and will be merged
 * with the default value property-by-property. Optional.
 */
const schemaObjectDefault = (schema: JsonObjectSchema, value?: any) => {
  // If the value is null, this is a special case where we want to return null.
  if (value === null) return null;

  // If properties are specified, we will, at minimum, provide those properties even if not required.
  if (schema.properties !== undefined) {
    const def: Record<string, any> = {};
    Object.keys(schema.properties).forEach((property) => {
      // We create each necessary property individually while merging with `value` if possible.
      def[property] = schemaBaseDefault(
        objectSubschema(schema, property),
        value === undefined ? undefined : value[property]
      );
    });
    return def;
  }
  // If properties is not specified, we return no properties.
  else return (value ?? {}) as Record<string, any>;
};
/**
 * Computes a default value for the specified array schema.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type and contain
 * the correct primitive types.
 * Note: this default value will satisfy the minimum number of items specified by the schema.
 * @param schema The array schema to generate a default value for.
 * @param value The existing value to merge with the default value. This value should be an array and will be merged
 * with the default value element-by-element. Optional.
 */
const schemaArrayDefault = (schema: JsonArraySchema, value?: any) => {
  // If there is no minimum number of items, we can default to an empty array.
  if (schema.minItems === undefined) return (value as any[]) ?? [];
  // Otherwise, we need to create at least the minimum number of elements.
  else {
    return Array(schema.minItems)
      .fill(null)
      .map((_, index) => {
        // We create each necessary element individually while merging with `value` if possible.
        return schemaBaseDefault(
          arraySubschema(schema, index),
          value === undefined ? undefined : value[index]
        );
      });
  }
};
/**
 * Computes a default schema for the specified number schema. Returns undefined if schema is impossible.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type.
 * Note: will default to a number that attempts to satisfy minimum, maximum, and multiple of.
 * @param schema The number schema to generate a default value for.
 * @param value The existing value to merge with the default value. Optional.
 */
const schemaNumberDefault = (schema: JsonNumberSchema, value?: any) => {
  if (value !== undefined) return value as number;

  // If the minimum is undefined and the maximum is undefined, specify zero (satisfies multiple of).
  if (schema.minimum === undefined && schema.maximum === undefined) return 0;

  // If the minimum is defined and the maximum is undefined, start at minimum and increment until valid.
  // If the minimum is undefined and the maximum is defined, start at the maximum and decrement until valid.
  // If the minimum is defined and the maximum is defined, start at the minimum and increment until valid.
  let defaultValue = schema.minimum ?? (schema.maximum as number);
  const direction = schema.minimum === undefined ? -1 : +1;
  do {
    let multipleOf = schema.multipleOf ?? 1;
    const minSatisfied =
      schema.minimum === undefined ||
      (!schema.exclusiveMinimum && defaultValue >= schema.minimum) ||
      (schema.exclusiveMinimum && defaultValue > schema.minimum);
    const maxSatisfied =
      schema.maximum === undefined ||
      (!schema.exclusiveMaximum && defaultValue <= schema.maximum) ||
      (schema.exclusiveMaximum && defaultValue < schema.maximum);
    const multipleSatisfied =
      schema.multipleOf === undefined ||
      Math.abs(defaultValue % multipleOf) < 1e-4;

    if (minSatisfied && maxSatisfied && multipleSatisfied) return defaultValue;

    const multipleBase =
      schema.multipleOf === undefined
        ? defaultValue
        : multipleOf * Math.floor(defaultValue / multipleOf);
    defaultValue = multipleBase + direction * multipleOf;
    defaultValue =
      schema.maximum === undefined
        ? defaultValue
        : Math.min(defaultValue, schema.maximum);
  } while (schema.maximum === undefined || defaultValue < schema.maximum);
};
/**
 * Computes a default value for the specified string schema.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type.
 * Note: will default to empty string for ease of filling in forms. Use a placeholder to provide syntax for a value.
 * @param schema The string schema to generate a default value for.
 * @param value The existing value to merge with the default value. Optional.
 */
const schemaStringDefault = (schema: JsonStringSchema, value?: any) => {
  // TODO: Add support for some common patterns.
  // We default to an empty string for now instead of using any sort of patterns.
  // Note: if the minimum or maximum lengths are set, an empty string might not be valid.
  // However, this default is meant for ease of filling in forms with validly typed values.
  if (value !== undefined) return value as string;
  return "";
};
/**
 * Computes a default value for the specified boolean schema.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type.
 * Note: will default to false since true most commonly indicates an additional feature.
 * @param schema The boolean schema to generate a default value for.
 * @param value The existing value to merge with the default value. Optional.
 */
const schemaBooleanDefault = (schema: JsonBooleanSchema, value?: any) => {
  // We will assume that true indicates something more special/active than false.
  // Thus, false is a "safer" default.
  if (value !== undefined) return value as boolean;
  return false;
};
/**
 * Computes a default value for the specified enumeration schema. Returns undefined if schema is impossible.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type.
 * Note: will use the first enumeration value if an existing value is not specified.
 * @param schema The enumeration schema to generate a default value for.
 * @param value The existing value to merge with the default value. Optional.
 */
const schemaEnumDefault = (schema: JsonEnumSchema, value?: any) => {
  // The scheme is enum, so we just return the first enum element.
  // If the enum is vacuumous (empty), we return undefined because the schema is not well defined.
  if (value !== undefined) return value;
  if (schema.enum.length === 0) return undefined;
  else return schema.enum[0];
};
/**
 * Computes a default value for the specified null schema.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type.
 * @param schema The null schema to generate a default value for.
 * @param value The existing value to merge with the default value. Optional.
 */
const schemaNullDefault = (schema: JsonNullSchema, value?: any) => {
  // The default for null is obviously null.
  if (value !== undefined) return value as null;
  return null;
};

/**
 * Computes a default value for the specified base schema. Returns undefined if schema is impossible.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type.
 * @param schema The base schema to generate a default value for.
 * @param value The existing value to merge with the default value. Optional.
 */
const schemaBaseDefault = (schema: JsonBaseSchema, value?: any): any => {
  // If a default was specified in the schema, use that value.
  if (schema.default !== undefined) return schema.default;

  if ("enum" in schema) {
    // Enum types need to be handled slightly differently because they can take on different primitive types.
    return schemaEnumDefault(schema as JsonEnumSchema, value);
  } else {
    if (Array.isArray(schema.type)) {
      // The scheme has multiple types, so we just return the default for the most specific value.
      // If the types are vacuumous (empty), we return undefined because the schema is not well defined.
      const specificity: JsonSchemaBasicTypename[] = [
        "null",
        "boolean",
        "string",
        "integer",
        "number",
        "array",
        "object",
      ];
      if (schema.type.length === 0) return undefined;
      else {
        for (let k = 0; k < specificity.length; k++) {
          if (schema.type.includes(specificity[k]))
            return schemaBaseDefault(
              { ...schema, type: specificity[k] },
              value
            );
        }
        return undefined;
      }
    } else {
      // The schema has a single type, so we can return its corresponding type default.
      switch (schema.type) {
        case "object":
          return schemaObjectDefault(schema as JsonObjectSchema, value);
        case "array":
          return schemaArrayDefault(schema as JsonArraySchema, value);
        case "integer":
        case "number":
          return schemaNumberDefault(schema as JsonNumberSchema, value);
        case "string":
          return schemaStringDefault(schema as JsonStringSchema, value);
        case "boolean":
          return schemaBooleanDefault(schema as JsonBooleanSchema, value);
        case "null":
          return schemaNullDefault(schema as JsonNullSchema, value);
      }
    }
  }
};

/**
 * Computes a default value for the specified schema.
 * Note: this default value need not validate automatically. It need only be of the correct primitive type.
 * @param schema The schema to generate a default value for.
 * @param value The existing value to merge with the default value. Optional.
 */
const schemaDefault = <T extends JsonSchema>(schema: T, value?: any) => {
  return schemaBaseDefault(schema, value);
};

// Export the methods to compute defaults from schema.
export default schemaDefault;
export {
  schemaObjectDefault,
  schemaArrayDefault,
  schemaNumberDefault,
  schemaStringDefault,
  schemaBooleanDefault,
  schemaEnumDefault,
  schemaNullDefault,
};
