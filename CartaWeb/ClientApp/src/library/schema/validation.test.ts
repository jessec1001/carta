import validateSchema from "./validation";

test("validates for null schema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "null",
      },
      null
    );

  expect(tester).not.toThrowError();
});
test("does not validate for null schema with incorrect type", () => {
  const tester = () =>
    validateSchema(
      {
        type: "null",
      },
      123
    );

  expect(tester).toThrowError();
});

test("validates for enum schema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "string",
        enum: ["apple", "banana", "cherry"],
      },
      "banana"
    );

  expect(tester).not.toThrowError();
});
test("does not validate for enum schema with non-included value", () => {
  const tester = () =>
    validateSchema(
      {
        type: undefined,
        enum: [1, 2, 3],
      },
      4
    );

  expect(tester).toThrowError();
});

test("validates for boolean schema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "boolean",
      },
      true
    );

  expect(tester).not.toThrowError();
});
test("does not validate for boolean schema with incorrect type", () => {
  const tester = () =>
    validateSchema(
      {
        type: "boolean",
      },
      "true"
    );

  expect(tester).toThrowError();
});

test("validates for string schema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "string",
      },
      "banana"
    );

  expect(tester).not.toThrowError();
});
test("does not validate for string schema with incorrect type", () => {
  const tester = () =>
    validateSchema(
      {
        type: "string",
      },
      ["banana"]
    );

  expect(tester).toThrowError();
});
test("validates for string schema with min length", () => {
  const tester = () =>
    validateSchema(
      {
        type: "string",
        minLength: 2,
      },
      "cherry"
    );

  expect(tester).not.toThrowError();
});
test("does not validate for string schema with min length", () => {
  const tester = () =>
    validateSchema(
      {
        type: "string",
        minLength: 3,
      },
      "ok"
    );

  expect(tester).toThrowError();
});
test("validates for string schema with max length", () => {
  const tester = () =>
    validateSchema(
      {
        type: "string",
        maxLength: 5,
      },
      "date"
    );

  expect(tester).not.toThrowError();
});
test("does not validate for string schema with max length", () => {
  const tester = () =>
    validateSchema(
      {
        type: "string",
        maxLength: 5,
      },
      "pineapple"
    );

  expect(tester).toThrowError();
});

test("validates for number schema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
      },
      1.25
    );

  expect(tester).not.toThrowError();
});
test("does not validate for number schema with incorrect type", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
      },
      "1.5"
    );

  expect(tester).toThrowError();
});
test("validates for number schema with multiple of", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
        multipleOf: 0.25,
      },
      1.25
    );

  expect(tester).not.toThrowError();
});
test("does not validate for number schema with multiple of", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
        multipleOf: 2,
      },
      3
    );

  expect(tester).toThrowError();
});
test("validates for number schema with inclusive minimum", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
        minimum: -1,
      },
      -1
    );

  expect(tester).not.toThrowError();
});
test("does not validate for number schema with inclusive minimum", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
        minimum: -1,
      },
      -2
    );

  expect(tester).toThrowError();
});
test("validates for number schema with exclusive maximum", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
        maximum: 10,
        exclusiveMaximum: true,
      },
      9.95
    );

  expect(tester).not.toThrowError();
});
test("does not validate for number schema with exclusive minimum", () => {
  const tester = () =>
    validateSchema(
      {
        type: "number",
        maximum: 10,
        exclusiveMaximum: true,
      },
      10
    );

  expect(tester).toThrowError();
});

test("validates for array schema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
      },
      []
    );

  expect(tester).not.toThrowError();
});
test("does not validate for array schema with incorrect type", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
      },
      {}
    );

  expect(tester).toThrowError();
});
test("validates for array schema with min items", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
        minItems: 3,
      },
      [1, 2, 3]
    );

  expect(tester).not.toThrowError();
});
test("does not validate for array schema with min items", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
        minItems: 3,
      },
      ["a", "b"]
    );

  expect(tester).toThrowError();
});
test("validates for array schema with max items", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
        maxItems: 3,
      },
      []
    );

  expect(tester).not.toThrowError();
});
test("does not validate for array schema with max items", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
        maxItems: 3,
      },
      [0, 1, 2, 3]
    );

  expect(tester).toThrowError();
});
test("validates for array schema with unique items", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
        uniqueItems: true,
      },
      [1, 2, 3]
    );

  expect(tester).not.toThrowError();
});
test("does not validate for array schema with unique items", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
        uniqueItems: true,
      },
      [1, 2, 3, 1]
    );

  expect(tester).toThrowError();
});
test("does not validate for array subschema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "array",
        items: {
          type: "string",
        },
      },
      ["apple", "banana", 5]
    );

  expect(tester).toThrowError();
});

test("validates for object schema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
      },
      {}
    );

  expect(tester).not.toThrowError();
});
test("does not validate for object schema with incorrect type", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
      },
      []
    );

  expect(tester).not.toThrowError();
});
test("validates for object schema with min properties", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
        minProperties: 2,
      },
      {
        foo: 1,
        bar: 2,
      }
    );

  expect(tester).not.toThrowError();
});
test("does not validate for object schema with min properties", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
        minProperties: 2,
      },
      {
        baz: "qux",
      }
    );

  expect(tester).toThrowError();
});
test("validates for object schema with max properties", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
        maxProperties: 2,
      },
      {
        a: 1,
        b: 2,
      }
    );

  expect(tester).not.toThrowError();
});
test("does not validate for object schema with max properties", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
        maxProperties: 3,
      },
      {
        apple: "a",
        banana: "b",
        cherry: "c",
        date: "d",
      }
    );

  expect(tester).toThrowError();
});
test("validates for object schema with required", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
        required: ["foo"],
      },
      {
        foo: true,
      }
    );

  expect(tester).not.toThrowError();
});
test("does not validate for object schema with required", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
        required: ["foo"],
      },
      {
        food: false,
      }
    );

  expect(tester).toThrowError();
});
test("does not validate for object subschema", () => {
  const tester = () =>
    validateSchema(
      {
        type: "object",
        properties: {
          test: {
            type: "number",
          },
        },
      },
      {
        test: "123",
      }
    );

  expect(tester).toThrowError();
});
