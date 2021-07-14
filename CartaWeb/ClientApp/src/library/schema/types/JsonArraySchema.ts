import {
  anySchema,
  neverSchema,
  JsonBaseSchema,
  JsonBaseTypedSchema,
} from "../JsonBaseSchema";

/** The type of widgets that can be used to input a JSON array. */
enum JsonArraySchemaWidgets {
  List = "list",
}

/** The schema specification for a JSON array. */
interface JsonArraySchema extends JsonBaseTypedSchema<"array", any[]> {
  items?: JsonBaseSchema | JsonBaseSchema[];
  additionalItems?: JsonBaseSchema | boolean;
  minItems?: number;
  maxItems?: number;
  uniqueItems?: boolean;

  "ui:widget"?: JsonArraySchemaWidgets;
}

/**
 * Computes a subschema at a given index in an array schema.
 * @param schema The array schema.
 * @param index The index of the array.
 */
const arraySubschema = (
  schema: JsonArraySchema,
  index: number
): JsonBaseSchema => {
  if (schema.items === undefined) {
    // The items schema/schemas is undefined so we need to determine if we should tap into the additional items schema.

    // If there is no max items, we don't use additional items.
    if (schema.maxItems === undefined) return anySchema;
    // If the max items is greater than our current index, we don't use additional items.
    else if (schema.maxItems > index) return anySchema;
  } else {
    // If the items schema is an array, we need to use the index of the array and treat the type like a tuple.
    if (Array.isArray(schema.items)) {
      // If the max items is greater than our current index, and
      // the items schema has an element corresponding to the index, we use that subschema.
      if (schema.maxItems === undefined || schema.maxItems > index) {
        if (schema.items.length > index) return schema.items[index];
      }
    }
    // Otherwise, the items schema is an actual schema.
    // We need to check if we've surpassed the the item limit before returning the schema.
    else {
      if (schema.maxItems === undefined || schema.maxItems > index)
        return schema.items;
    }
  }

  // If no previous conditions have been met, we try to use the additional items schema.
  // If the additional items schema is undefined, assume more elements are allowed.
  if (schema.additionalItems === undefined) return anySchema;
  // If the additional items schema is a boolean, we allow more elements if true and no more elements otherwise.
  else if (typeof schema.additionalItems === "boolean") {
    if (schema.additionalItems) return anySchema;
    else return neverSchema;
  }
  // If the additional items schema is an actual schema, use it.
  else return schema.additionalItems;
};

export default JsonArraySchema;
export { arraySubschema, JsonArraySchemaWidgets };
