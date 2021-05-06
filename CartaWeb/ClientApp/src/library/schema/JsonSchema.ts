interface JsonSchemaProperty {
  title?: string;
  description?: string;
}
interface JsonSchemaObject extends JsonSchemaProperty {
  type: "object";
  properties?: Record<string, JsonSchemaType>;
  required?: Array<string>;
}
interface JsonSchemaInteger extends JsonSchemaProperty {
  type: "integer";
}
interface JsonSchemaString extends JsonSchemaProperty {
  type: "string";
}
type JsonSchemaType = JsonSchemaObject | JsonSchemaInteger | JsonSchemaString;
type JsonSchema = JsonSchemaType & {
  $schema: string;
  $id: string;
};
// TODO: required list should only contain keys in properties.

const testSchema: JsonSchema = {
  $schema: "https://json-schema.org/draft/2020-12/schema",
  $id: "https://example.com/product.schema.json",
  title: "Product",
  description: "A product in the catalog",
  type: "object",
  properties: {
    productId: {
      title: "ID",
      description: "The unique identifier for a product",
      type: "integer",
    },
    productName: {
      title: "Name",
      description: "The name of the product",
      type: "string",
    },
  },
  required: ["productId", "productName"],
};

export default JsonSchema;
export type {
  JsonSchemaType,
  JsonSchemaObject,
  JsonSchemaInteger,
  JsonSchemaString,
};
