import schemaDefault from "./defaults";

test("default for null schema", () => {
  const defaultValue = schemaDefault({
    type: "null",
  });

  expect(defaultValue).toBeNull();
});

test("default for string enum schema", () => {
  const defaultValue = schemaDefault({
    type: "string",
    enum: ["apple", "banana", "cherry"],
  });

  expect(defaultValue).toBe("apple");
});
test("default for int enum schema", () => {
  const defaultValue = schemaDefault({
    type: "integer",
    enum: [3, 2, 1],
  });

  expect(defaultValue).toBe(3);
});
test("default for invalid enum schema", () => {
  const defaultValue = schemaDefault({
    type: "integer",
    enum: [],
  });

  expect(defaultValue).toBeUndefined();
});

test("default for boolean schema", () => {
  const defaultValue = schemaDefault({
    type: "boolean",
  });

  expect(defaultValue).toBe(false);
});
test("default for true boolean schema", () => {
  const defaultValue = schemaDefault({
    type: "boolean",
    default: true,
  });

  expect(defaultValue).toBe(true);
});

test("default for string schema", () => {
  const defaultValue = schemaDefault({
    type: "string",
  });

  expect(defaultValue).toBe("");
});
test("default for lengthy string schema", () => {
  const defaultValue = schemaDefault({
    type: "string",
    minLength: 5,
  });

  expect(defaultValue).toBe("");
});

test("default for number schema", () => {
  const defaultValue = schemaDefault({
    type: "integer",
  });

  expect(defaultValue).toBe(0);
});
test("default for number schema with inclusive minimum", () => {
  const defaultValue = schemaDefault({
    type: "number",
    minimum: 4.5,
  });

  expect(defaultValue).toBeCloseTo(4.5);
});
test("default for number schema with inclusive minimum and multiple of", () => {
  const defaultValue = schemaDefault({
    type: "number",
    minimum: 1.1,
    multipleOf: 0.25,
  });

  expect(defaultValue).toBeCloseTo(1.25);
});
test("default for number schema with inclusive maximum", () => {
  const defaultValue = schemaDefault({
    type: "integer",
    maximum: -2,
  });

  expect(defaultValue).toBeCloseTo(-2);
});
test("default for number schema with inclusive maximum and multiple of", () => {
  const defaultValue = schemaDefault({
    type: "number",
    maximum: -2.1,
    multipleOf: 0.3,
  });

  expect(defaultValue).toBeCloseTo(-2.1);
});
test("default for number schema with exclusive minimum", () => {
  const defaultValue = schemaDefault({
    type: "number",
    minimum: 3,
    exclusiveMinimum: true,
  });

  expect(defaultValue).toBeCloseTo(4);
});
test("default for number schema with exclusive minimum and multiple of", () => {
  const defaultValue = schemaDefault({
    type: "integer",
    minimum: 4,
    exclusiveMinimum: true,
    multipleOf: 2,
  });

  expect(defaultValue).toBeCloseTo(6);
});
test("default for number schema with exclusive maximum", () => {
  const defaultValue = schemaDefault({
    type: "integer",
    maximum: 6,
    exclusiveMaximum: true,
  });

  expect(defaultValue).toBeCloseTo(5);
});
test("default for number schema with exclusive maximum and multiple of", () => {
  const defaultValue = schemaDefault({
    type: "number",
    maximum: -1.1,
    exclusiveMaximum: true,
    multipleOf: 0.1,
  });

  expect(defaultValue).toBeCloseTo(-1.2);
});
test("default for invalid number schema with inclusive bounds", () => {
  const defaultValue = schemaDefault({
    type: "number",
    minimum: 2,
    maximum: 1,
  });

  expect(defaultValue).toBeUndefined();
});
test("default for invalid number schema with exclusive bounds", () => {
  const defaultValue = schemaDefault({
    type: "number",
    minimum: -1,
    maximum: -1,
    exclusiveMinimum: true,
    exclusiveMaximum: true,
  });

  expect(defaultValue).toBeUndefined();
});
test("default for number schema with inclusive bounds", () => {
  const defaultValue = schemaDefault({
    type: "number",
    minimum: 3.1,
    maximum: 3.1,
  });

  expect(defaultValue).toBeCloseTo(3.1);
});

test("default for empty number array schema", () => {
  const defaultValue = schemaDefault({
    type: "array",
    items: {
      type: "number",
    },
  });

  expect(defaultValue).toHaveLength(0);
});
test("default for nonempty number array schema", () => {
  const defaultValue = schemaDefault({
    type: "array",
    items: {
      type: "number",
      minimum: -1,
    },
    minItems: 3,
  });

  expect(defaultValue).toHaveLength(3);
  [-1, -1, -1].forEach((defaultSubvalue, index) =>
    expect(defaultSubvalue).toBe(defaultValue[index])
  );
});
test("default for tuple items array schema", () => {
  const defaultValue = schemaDefault({
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
      {
        type: "boolean",
        default: true,
      },
    ],
    minItems: 4,
  });

  expect(defaultValue).toHaveLength(4);
  ["", 0, false, true].forEach((defaultSubvalue, index) =>
    expect(defaultSubvalue).toBe(defaultValue[index])
  );
});
test("default for additional items array schema", () => {
  const defaultValue = schemaDefault({
    type: "array",
    items: [
      {
        type: "number",
        maximum: 4,
      },
    ],
    additionalItems: {
      type: "number",
      minimum: -1,
    },
    minItems: 3,
  });

  expect(defaultValue).toHaveLength(3);
  [4, -1, -1].forEach((defaultSubvalue, index) =>
    expect(defaultSubvalue).toBe(defaultValue[index])
  );
});
test("default with merging string array schema", () => {
  const defaultValue = schemaDefault(
    {
      type: "array",
      items: {
        type: "number",
      },
      minItems: 4,
    },
    [1, 2]
  );

  expect(defaultValue).toHaveLength(4);
  [1, 2, 0, 0].forEach((defaultSubvalue, index) =>
    expect(defaultSubvalue).toBe(defaultValue[index])
  );
});

test("default for empty object schema", () => {
  const defaultValue = schemaDefault({
    type: "object",
  });

  expect(Object.keys(defaultValue)).toHaveLength(0);
});
test("default for nonempty object schema", () => {
  const defaultValue = schemaDefault({
    type: "object",
    properties: {
      prop1: {
        type: "number",
      },
      prop2: {
        type: "string",
      },
    },
  });

  expect(Object.keys(defaultValue)).toHaveLength(2);
  expect(defaultValue.prop1).toBe(0);
  expect(defaultValue.prop2).toBe("");
});
test("default with merging object schema", () => {
  const defaultValue = schemaDefault(
    {
      type: "object",
      properties: {
        foo: {
          enum: ["bip", "bap"],
        },
        bar: {
          type: "array",
          items: {
            type: "number",
          },
          minItems: 2,
        },
      },
    },
    { bar: [1] }
  );

  expect(Object.keys(defaultValue)).toHaveLength(2);
  expect(defaultValue.foo).toBe("bip");
  expect(defaultValue.bar).toHaveLength(2);
  expect(defaultValue.bar[0]).toBe(1);
  expect(defaultValue.bar[1]).toBe(0);
});
