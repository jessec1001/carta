import { JsonSchema } from "./JsonSchema";
import { JsonBaseSchema, JsonSchemaBasicTypename } from "./JsonBaseSchema";
import {
  arraySubschema,
  JsonArraySchema,
  JsonBooleanSchema,
  JsonEnumSchema,
  JsonNullSchema,
  JsonNumberSchema,
  JsonObjectSchema,
  JsonStringSchema,
  objectSubschema,
} from "./types";
import { escapeRegex } from "library/utility";

/** Represents an {@link Error} that is emitted when validating a value against a schema. */
class ValidationError extends Error {
  /** A stack trace of nested objects or arrays that indicates where the validation error takes place. */
  public trace: (string | number)[];

  constructor(trace: (string | number)[], message?: string, ...params: []) {
    super(message, ...params);

    // Maintain proper stack trace for this error.
    if (Error.captureStackTrace) {
      Error.captureStackTrace(this, ValidationError);
    }

    this.trace = trace;
  }
}

/**
 * Validates a value against the specified object schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaObject = (
  schema: JsonObjectSchema,
  value: any,
  trace: (string | number)[]
) => {
  // Check type of value.
  if (!validateSchemaType<any>(schema.type, value, trace)) return;

  // Check length of value.
  if (
    schema.maxProperties !== undefined &&
    Object.keys(value).length > schema.maxProperties
  )
    throw new ValidationError(
      trace,
      `Value must have maximum of ${schema.maxProperties} properties.`
    );
  if (
    schema.minProperties !== undefined &&
    Object.keys(value).length < schema.minProperties
  )
    throw new ValidationError(
      trace,
      `Value must have a minimum of ${schema.minProperties} properties.`
    );

  // Check required properties.
  if (schema.required !== undefined) {
    schema.required.forEach((property) => {
      if (!(property in value))
        throw new ValidationError(trace, `"${property}" value is required.`);
    });
  }

  // Check subvalues against subschemas.
  Object.entries(value).forEach(([property, subvalue]) => {
    const subschema = objectSubschema(schema, property);
    validateSchemaBase(subschema, subvalue, [...trace, property]);
  });
};
/**
 * Validates a value against the specified array schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaArray = (
  schema: JsonArraySchema,
  value: any,
  trace: (string | number)[]
) => {
  // Check type of value.
  if (!validateSchemaType<any[]>(schema.type, value, trace)) return;

  // Check length of value.
  if (schema.maxItems !== undefined && value.length > schema.maxItems)
    throw new ValidationError(
      trace,
      `Value must have maximum of ${schema.maxItems} items.`
    );
  if (schema.minItems !== undefined && value.length < schema.minItems)
    throw new ValidationError(
      trace,
      `Value must have minimum of ${schema.minItems} items.`
    );

  // Check unique subvalues.
  if (schema.uniqueItems) {
    const uniqueValues = new Set(value);
    if (uniqueValues.size !== value.length)
      throw new ValidationError(trace, `Value must have unique items.`);
  }

  // Check subvalues against subschemas.
  value.forEach((subvalue, index) => {
    const subschema = arraySubschema(schema, index);
    validateSchemaBase(subschema, subvalue, [...trace, index]);
  });
};
/**
 * Validates a value against the specified number schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaNumber = (
  schema: JsonNumberSchema,
  value: any,
  trace: (string | number)[]
) => {
  // Check type of value.
  if (!validateSchemaType<number>(schema.type, value, trace)) return;

  // Check against NaN.
  if (isNaN(value)) throw new ValidationError(trace, `Value is not a number.`);

  // Check minimum value.
  if (schema.minimum !== undefined) {
    if (schema.exclusiveMinimum && value <= schema.minimum)
      throw new ValidationError(
        trace,
        `Value must have exlusive minimum of ${schema.minimum}.`
      );
    if (!schema.exclusiveMinimum && value < schema.minimum)
      throw new ValidationError(
        trace,
        `Value must have inclusive minimum of ${schema.minimum}.`
      );
  }

  // Check maximum value.
  if (schema.maximum !== undefined) {
    if (schema.exclusiveMaximum && value >= schema.maximum)
      throw new ValidationError(
        trace,
        `Value must have exlusive maximum of ${schema.maximum}.`
      );
    if (!schema.exclusiveMaximum && value > schema.maximum)
      throw new ValidationError(
        trace,
        `Value must have inclusive maximum of ${schema.maximum}.`
      );
  }

  // Check multiple of value.
  const multipleOf =
    schema.multipleOf ?? (schema.type === "integer" ? 1 : undefined);
  if (
    multipleOf !== undefined &&
    Math.abs(value % multipleOf) >= Math.abs(multipleOf / (1 << 4))
  )
    throw new ValidationError(
      trace,
      `Value must be a multiple of ${schema.multipleOf}.`
    );

  // TODO: Validate against format property.
  // schema.format;
};
/**
 * Validates a value against the specified string schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaString = (
  schema: JsonStringSchema,
  value: any,
  trace: (string | number)[]
) => {
  // Check type of value.
  if (!validateSchemaType<string>(schema.type, value, trace)) return;

  // Check length of value.
  if (schema.maxLength !== undefined && schema.maxLength < value.length)
    throw new ValidationError(
      trace,
      `Value must have maximum length of ${schema.maxLength}.`
    );
  if (schema.minLength !== undefined && schema.minLength > value.length)
    throw new ValidationError(
      trace,
      `Value must have minimum length of ${schema.minLength}.`
    );

  if (schema.format) {
    switch (schema.format) {
      case "regex":
        try {
          new RegExp(escapeRegex(value));
        } catch {
          throw new ValidationError(
            trace,
            `Value is not a valid regular expression.`
          );
        }
        break;
    }
  }

  // TODO: Validate against pattern property.
  // schema.pattern;
};
/**
 * Validates a value against the specified boolean schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaBoolean = (
  schema: JsonBooleanSchema,
  value: any,
  trace: (string | number)[]
) => {
  // Check type of value.
  if (!validateSchemaType<boolean>(schema.type, value, trace)) return;
};
/**
 * Validates a value against the specified enumeration schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaEnum = (
  schema: JsonEnumSchema,
  value: any,
  trace: (string | number)[]
) => {
  // TODO: Validate against enum type property.
  // schema.type;

  // Check inclusion in valid enumeration values.
  const enumIndex = schema.enum.findIndex((enumValue) =>
    Object.is(value, enumValue)
  );
  if (enumIndex < 0)
    throw new ValidationError(
      trace,
      `Expected value to be one of (${schema.enum.join(", ")})`
    );
};
/**
 * Validates a value against the specified null schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaNull = (
  schema: JsonNullSchema,
  value: any,
  trace: (string | number)[]
) => {
  // Check type of value.
  if (!validateSchemaType<null>(schema.type, value, trace)) return;
};

const validateSchemaType = <T>(
  type: JsonSchemaBasicTypename,
  value: any,
  trace: (string | number)[]
): value is T => {
  switch (type) {
    case "object":
      if (typeof value !== "object")
        throw new ValidationError(
          trace,
          `Expected value to be of type "object".`
        );
      break;
    case "array":
      if (!Array.isArray(value))
        throw new ValidationError(
          trace,
          `Expected value to be of type "array".`
        );
      break;
    case "integer":
    case "number":
      if (typeof value !== "number")
        throw new ValidationError(
          trace,
          `Expected value to be of type "number".`
        );
      break;
    case "string":
      if (typeof value !== "string")
        throw new ValidationError(
          trace,
          `Expected value to be of type "string".`
        );
      break;
    case "boolean":
      if (typeof value !== "boolean")
        throw new ValidationError(
          trace,
          `Expected value to be of type "boolean".`
        );
      break;
    case "null":
      if (value !== null)
        throw new ValidationError(
          trace,
          `Expected value to be of type "null".`
        );
      break;
  }
  return true;
};

/**
 * Validates a value against the specified base schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 * @param trace The trace stack of nested keys.
 */
const validateSchemaBase = (
  schema: JsonBaseSchema,
  value: any,
  trace: (string | number)[]
) => {
  if ("enum" in schema) {
    // Enum types need to be handled slightly differently because they can take on different primitive types.
    return validateSchemaEnum(schema as JsonEnumSchema, value, trace);
  } else {
    if (Array.isArray(schema.type)) {
      // The schema has multiple types, so we check if the value validates against any of them.
      // If it does validate for any type, the value if validated entirely.
      let typeValidated: boolean = false;
      let type: JsonSchemaBasicTypename | null = null;
      for (let k = 0; k < schema.type.length; k++) {
        type = schema.type[k];
        try {
          validateSchemaType(type, value, trace);
          typeValidated = true;
          break;
        } catch (err) {
          if (!(err instanceof ValidationError)) throw err;
        }
      }

      // If we've reached this point without validating, we didn't match a type.
      if (!typeValidated)
        throw new ValidationError(
          trace,
          `Expected value to be one of the types (${schema.type.join(", ")}).`
        );
      // We need to make sure we have a type inferred before validating its value.
      if (type === null)
        throw new ValidationError(
          trace,
          `Value does not have any valid types.`
        );

      // We finally validate the value against a new schema with a specific type.
      validateSchemaBase({ ...schema, type }, value, trace);
    } else {
      // The schema has a single type, so we can validate its corresponding type only.
      switch (schema.type) {
        case "object":
          validateSchemaObject(schema as JsonObjectSchema, value, trace);
          break;
        case "array":
          validateSchemaArray(schema as JsonArraySchema, value, trace);
          break;
        case "integer":
        case "number":
          validateSchemaNumber(schema as JsonNumberSchema, value, trace);
          break;
        case "string":
          validateSchemaString(schema as JsonStringSchema, value, trace);
          break;
        case "boolean":
          validateSchemaBoolean(schema as JsonBooleanSchema, value, trace);
          break;
        case "null":
          validateSchemaNull(schema as JsonNullSchema, value, trace);
          break;
      }
    }
  }
};

/**
 * Validates a value against the specified schema.
 * Throws a {@link ValidationError} if validation fails.
 * @param schema The schema to validate against.
 * @param value The value to validate.
 */
const validateSchema = <T extends JsonSchema>(schema: T, value: any) => {
  validateSchemaBase(schema, value, []);
};

// Export the methods to validate values against schemas.
export default validateSchema;
export {
  validateSchemaObject,
  validateSchemaArray,
  validateSchemaNumber,
  validateSchemaString,
  validateSchemaBoolean,
  validateSchemaEnum,
  validateSchemaNull,
};
export { ValidationError };
