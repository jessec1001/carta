import { FunctionComponent } from "react";
import {
  JsonArraySchema,
  JsonBaseSchema,
  JsonBooleanSchema,
  JsonEnumSchema,
  JsonFileSchema,
  JsonMultitypeSchema,
  JsonNumberSchema,
  JsonObjectSchema,
  JsonStringSchema,
  ValidationError,
} from "library/schema";
import SchemaNumberInput from "./SchemaNumberInput";
import SchemaEnumInput from "./SchemaEnumInput";
import SchemaFileInput from "./SchemaFileInput";
import SchemaStringInput from "./SchemaStringInput";
import SchemaBooleanInput from "./SchemaBooleanInput";
import SchemaObjectInput from "./SchemaObjectInput";
import SchemaArrayInput from "./SchemaArrayInput";
import SchemaMultitypeInput from "./SchemaMultitypeInput";
import { NullSymbol } from "components/symbols";

interface SchemaTypedInputProps<
  TSchema extends JsonBaseSchema,
  TValue,
  TWidgets
> {
  schema: TSchema;
  widget?: TWidgets;

  error?: ValidationError;
  value?: TValue;
  onChange?: (value: TValue) => void;
}

type SchemaBaseInputProps = SchemaTypedInputProps<
  JsonBaseSchema,
  any,
  undefined
>;

const SchemaBaseInput: FunctionComponent<SchemaBaseInputProps> = ({
  schema,
  error,
  value,
  onChange,
  ...props
}) => {
  // We need the underlying input type to wrap in a form group.
  if (Array.isArray(schema.type)) {
    // An array of types indicates a multitype value.
    const multitypeSchema = schema as JsonMultitypeSchema;
    return (
      <SchemaMultitypeInput
        schema={multitypeSchema}
        error={error}
        value={value}
        onChange={onChange}
        {...props}
      />
    );
  } else {
    if ("enum" in schema) {
      // Having the enum property indicates an enumeration type value.
      const enumSchema = schema as JsonEnumSchema;
      return (
        <SchemaEnumInput
          schema={enumSchema}
          error={error}
          value={value}
          onChange={onChange}
          {...props}
        />
      );
    } else {
      // Any other type can be handled as a simple type value.
      switch (schema.type) {
        case "file":
          const fileSchema = schema as JsonFileSchema;
          return (
            <SchemaFileInput
              schema={fileSchema}
              error={error}
              value={value}
              onChange={onChange}
              {...props}
            />
          );
        case "object":
          const objectSchema = schema as JsonObjectSchema;
          return (
            <SchemaObjectInput
              schema={objectSchema}
              error={error}
              value={value}
              onChange={onChange}
              {...props}
            />
          );
        case "array":
          const arraySchema = schema as JsonArraySchema;
          return (
            <SchemaArrayInput
              schema={arraySchema}
              error={error}
              value={value}
              onChange={onChange}
              {...props}
            />
          );
        case "string":
          const stringSchema = schema as JsonStringSchema;
          return (
            <SchemaStringInput
              schema={stringSchema}
              error={error}
              value={value}
              onChange={onChange}
              {...props}
            />
          );
        case "integer":
        case "number":
          const numberSchema = schema as JsonNumberSchema;
          return (
            <SchemaNumberInput
              schema={numberSchema}
              error={error}
              value={value}
              onChange={onChange}
              {...props}
            />
          );
        case "boolean":
          const booleanSchema = schema as JsonBooleanSchema;
          return (
            <SchemaBooleanInput
              schema={booleanSchema}
              error={error}
              value={value}
              onChange={onChange}
              {...props}
            />
          );
        case "null":
        default:
          // In the case of null, there is no input so we just emit the null symbol.
          return <NullSymbol />;
      }
    }
  }
};

export default SchemaBaseInput;
export type { SchemaTypedInputProps, SchemaBaseInputProps };
