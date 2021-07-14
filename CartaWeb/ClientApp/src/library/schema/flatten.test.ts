import flattenSchema from "./flatten";
import { JsonSchema } from "./JsonSchema";

test("does not modify already flat", () => {
  const schema: JsonSchema = {
    type: "array",
    items: {
      type: "number",
    },
  } as JsonSchema;
  const schemaFlat: any = flattenSchema(schema);

  expect(schemaFlat.type).toBe("array");
  expect(schemaFlat.items).not.toBeUndefined();
  expect(schemaFlat.items.type).toBe("number");
});
test("flattens singular one of", () => {
  const schema: JsonSchema = {
    oneOf: [
      {
        type: "number",
        minimum: 1,
      },
    ],
  } as JsonSchema;
  const schemaFlat: any = flattenSchema(schema);

  expect(schemaFlat.type).toBe("number");
  expect(schemaFlat.minimum).toBe(1);
  expect(schemaFlat.oneOf).toBeUndefined();
});
test("flattens singular all of", () => {
  const schema: JsonSchema = {
    allOf: [
      {
        type: "string",
        maxLength: 10,
      },
    ],
  } as JsonSchema;
  const schemaFlat: any = flattenSchema(schema);

  expect(schemaFlat.type).toBe("string");
  expect(schemaFlat.maxLength).toBe(10);
  expect(schemaFlat.allOf).toBeUndefined();
});
test("flattens references", () => {
  const schema: JsonSchema = {
    definitions: {
      MyDef: {
        type: "boolean",
        default: true,
      },
    },
    $ref: "#/definitions/MyDef",
  } as JsonSchema;
  const schemaFlat: any = flattenSchema(schema);

  expect(schemaFlat.type).toBe("boolean");
  expect(schemaFlat.default).toBe(true);
  expect(schemaFlat.$ref).toBeUndefined();
});
test("flattens object properties", () => {
  const schema: JsonSchema = {
    type: "object",
    definitions: {
      MyDef: {
        type: "boolean",
        default: true,
      },
    },
    properties: {
      foo: {
        $ref: "#/definitions/MyDef",
      },
      bar: {
        oneOf: [
          {
            type: "number",
          },
        ],
      },
    },
  } as JsonSchema;
  const schemaFlat: any = flattenSchema(schema);

  expect(schemaFlat.type).toBe("object");
  expect(schemaFlat.properties).not.toBeUndefined();
  expect(schemaFlat.properties.foo).not.toBeUndefined();
  expect(schemaFlat.properties.foo.type).toBe("boolean");
  expect(schemaFlat.properties.foo.default).toBe(true);
  expect(schemaFlat.properties.bar).not.toBeUndefined();
  expect(schemaFlat.properties.bar.type).toBe("number");
});
test("flattens array items", () => {
  const schema: JsonSchema = {
    type: "array",
    definitions: {
      MyDef: {
        type: "boolean",
        default: true,
      },
    },
    items: [
      {
        oneOf: [
          {
            type: "number",
          },
        ],
      },
      {
        $ref: "#/definitions/MyDef",
      },
    ],
  } as JsonSchema;
  const schemaFlat: any = flattenSchema(schema);

  expect(schemaFlat.type).toBe("array");
  expect(schemaFlat.items).not.toBeUndefined();
  expect(schemaFlat.items[0]).not.toBeUndefined();
  expect(schemaFlat.items[0].type).toBe("number");
  expect(schemaFlat.items[1]).not.toBeUndefined();
  expect(schemaFlat.items[1].type).toBe("boolean");
  expect(schemaFlat.items[1].default).toBe(true);
});
