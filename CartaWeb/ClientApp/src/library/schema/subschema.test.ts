import { JsonBaseSchema } from "./JsonBaseSchema";
import {
  arraySubschema,
  objectSubschema,
  JsonArraySchema,
  JsonObjectSchema,
} from "./types";

test("array subschema with array items only", () => {
  const schema: JsonArraySchema = {
    type: "array",
    items: {
      type: "number",
      minimum: 3,
    } as JsonBaseSchema,
  };

  const subschema0: any = arraySubschema(schema, 0);
  expect(subschema0.type).toBe("number");
  expect(subschema0.minimum).toBe(3);

  const subschema1: any = arraySubschema(schema, 1);
  expect(subschema1.type).toBe("number");
  expect(subschema1.minimum).toBe(3);

  const subschema3: any = arraySubschema(schema, 3);
  expect(subschema3.type).toBe("number");
  expect(subschema3.minimum).toBe(3);
});
test("array subschema with array items and max items", () => {
  const schema: JsonArraySchema = {
    type: "array",
    items: {
      type: "string",
      minLength: 2,
    } as JsonBaseSchema,
    maxItems: 2,
    additionalItems: false,
  };

  const subschema0: any = arraySubschema(schema, 0);
  expect(subschema0.type).toBe("string");
  expect(subschema0.minLength).toBe(2);

  const subschema1: any = arraySubschema(schema, 1);
  expect(subschema1.type).toBe("string");
  expect(subschema1.minLength).toBe(2);

  const subschema3: any = arraySubschema(schema, 3);
  expect(Array.isArray(subschema3.type)).toBeTruthy();
  expect(subschema3.type).toHaveLength(0);
});
test("array subschema with tuple items", () => {
  const schema: JsonArraySchema = {
    type: "array",
    items: [
      {
        type: "string",
      },
      {
        type: "number",
      },
      {
        type: "boolean",
      },
    ],
    maxItems: 2,
    additionalItems: false,
  };

  const subschema0: any = arraySubschema(schema, 0);
  expect(subschema0.type).toBe("string");

  const subschema1: any = arraySubschema(schema, 1);
  expect(subschema1.type).toBe("number");

  const subschema3: any = arraySubschema(schema, 3);
  expect(Array.isArray(subschema3.type)).toBeTruthy();
  expect(subschema3.type).toHaveLength(0);
});
test("array subschema with additional schema items", () => {
  const schema: JsonArraySchema = {
    type: "array",
    items: { type: "string" },
    maxItems: 2,
    additionalItems: { type: "number" },
  };

  const subschema0: any = arraySubschema(schema, 0);
  expect(subschema0.type).toBe("string");

  const subschema1: any = arraySubschema(schema, 1);
  expect(subschema1.type).toBe("string");

  const subschema3: any = arraySubschema(schema, 3);
  expect(subschema3.type).toBe("number");
});
test("array subschema with additional any items", () => {
  const schema: JsonArraySchema = {
    type: "array",
    items: { type: "string" },
    maxItems: 2,
    additionalItems: true,
  };

  const subschema0: any = arraySubschema(schema, 0);
  expect(subschema0.type).toBe("string");

  const subschema1: any = arraySubschema(schema, 1);
  expect(subschema1.type).toBe("string");

  const subschema3: any = arraySubschema(schema, 3);
  expect(Array.isArray(subschema3.type)).toBeTruthy();
  expect(subschema3.type.length).toBeGreaterThan(0);
});

test("object subschema with properties only", () => {
  const schema: JsonObjectSchema = {
    type: "object",
    properties: {
      foo: { type: "string", minLength: 2 } as JsonBaseSchema,
      bar: { type: "number", maximum: 10 } as JsonBaseSchema,
    },
    additionalProperties: false,
  };

  const subschemaFoo: any = objectSubschema(schema, "foo");
  expect(subschemaFoo.type).toBe("string");
  expect(subschemaFoo.minLength).toBe(2);

  const subschemaBar: any = objectSubschema(schema, "bar");
  expect(subschemaBar.type).toBe("number");
  expect(subschemaBar.maximum).toBe(10);

  const subschemaBaz: any = objectSubschema(schema, "baz");
  expect(Array.isArray(subschemaBaz.type)).toBeTruthy();
  expect(subschemaBaz.type).toHaveLength(0);
});
test("object subschema with additional schema items", () => {
  const schema: JsonObjectSchema = {
    type: "object",
    properties: {
      foo: { type: "string", minLength: 2 } as JsonBaseSchema,
      bar: { type: "number", maximum: 10 } as JsonBaseSchema,
    },
    additionalProperties: {
      type: "boolean",
    },
  };

  const subschemaFoo: any = objectSubschema(schema, "foo");
  expect(subschemaFoo.type).toBe("string");
  expect(subschemaFoo.minLength).toBe(2);

  const subschemaBar: any = objectSubschema(schema, "bar");
  expect(subschemaBar.type).toBe("number");
  expect(subschemaBar.maximum).toBe(10);

  const subschemaBaz: any = objectSubschema(schema, "baz");
  expect(subschemaBaz.type).toBe("boolean");
});
test("object subschema with additional any items", () => {
  const schema: JsonObjectSchema = {
    type: "object",
    properties: {
      foo: { type: "string", minLength: 2 } as JsonBaseSchema,
      bar: { type: "number", maximum: 10 } as JsonBaseSchema,
    },
    additionalProperties: true,
  };

  const subschemaFoo: any = objectSubschema(schema, "foo");
  expect(subschemaFoo.type).toBe("string");
  expect(subschemaFoo.minLength).toBe(2);

  const subschemaBar: any = objectSubschema(schema, "bar");
  expect(subschemaBar.type).toBe("number");
  expect(subschemaBar.maximum).toBe(10);

  const subschemaBaz: any = objectSubschema(schema, "baz");
  expect(Array.isArray(subschemaBaz.type)).toBeTruthy();
  expect(subschemaBaz.type.length).toBeGreaterThan(0);
});
