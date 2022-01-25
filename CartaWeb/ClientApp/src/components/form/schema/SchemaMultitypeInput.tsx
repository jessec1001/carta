import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import {
  JsonMultitypeSchema,
  JsonMultitypeSchemaWidgets,
  JsonSchemaBasicTypename,
  schemaDefault,
} from "library/schema";
import { toTitleCase } from "library/utility";
import { CheckboxInput, DropdownInput, OptionInput } from "components/input";
import SchemaBaseInput, { SchemaTypedInputProps } from "./SchemaBaseInput";

/** The props used for the {@link SchemaMultitypeInput} component. */
type SchemaMultitypeInputProps = SchemaTypedInputProps<
  JsonMultitypeSchema,
  any,
  JsonMultitypeSchemaWidgets
>;

/**
 * Infers the type of a value from a list of available types.
 * @param value The value to infer the type of.
 * @param types The possible types the value should be able to take on.
 * @returns The JSON type of the value or `null` if no type matched.
 */
const inferType = (
  value: any,
  types: JsonSchemaBasicTypename[]
): JsonSchemaBasicTypename | null => {
  // TODO: Temporary workaround to allow for file uploads.
  if (types.includes("file")) return "file";

  // For each type that is defined, check if it matches the value.
  // If so, return that type as a match.
  for (const type of types) {
    switch (type) {
      case "null":
        if (value === null) return type;
        break;
      case "boolean":
        if (typeof value === "boolean") return type;
        break;
      case "string":
        if (typeof value === "string") return type;
        break;
      case "integer":
        if (
          typeof value === "number" &&
          (isNaN(value) || Math.floor(value) === value)
        )
          return type;
        break;
      case "number":
        if (typeof value === "number") return type;
        break;
      case "array":
        if (Array.isArray(value)) return type;
        break;
      case "object":
        if (typeof value === "object") return type;
        break;
    }
  }

  // If we have not matched a type, return undefined to indicate to stop rendering.
  return null;
};

/** A component that inputs a multityped values based on a schema with a dropdown to select the type. */
const SchemaMultitypeInput: FunctionComponent<SchemaMultitypeInputProps> = ({
  schema,
  widget,
  error,
  value,
  onChange,
  children,
  ...props
}) => {
  // We allow the component to have an optionally controlled value.
  const [actualValue, setValue] = useControllableState(
    () => schemaDefault(schema, value),
    value,
    onChange
  );
  // We need to infer the type of value.
  const type = inferType(actualValue, schema.type);
  const typeOptions = schema.type;

  // We modify the schema to display the correct input.
  if (type === null) return null;
  const typedSchema = { ...schema, type };

  // Recomputes a default value whenever the type changes.
  const handleChangeType = (newType: JsonSchemaBasicTypename) => {
    const newTypedSchema = { ...schema, type: newType };
    setValue(schemaDefault(newTypedSchema));
  };

  // By default, we should display this input with a dropdown menu for the type.
  let uiWidget: JsonMultitypeSchemaWidgets =
    widget ?? (schema.type.length === 2 && schema.type.includes("null"))
      ? JsonMultitypeSchemaWidgets.Checkbox
      : JsonMultitypeSchemaWidgets.Dropdown;
  if (schema["ui:widget"] !== undefined) uiWidget = schema["ui:widget"];

  // Display based on decided upon widget.
  switch (uiWidget) {
    case JsonMultitypeSchemaWidgets.Dropdown:
      return (
        <div className="form-spaced-group">
          <div>
            <SchemaBaseInput
              schema={typedSchema}
              error={error}
              value={actualValue}
              onChange={setValue}
              {...props}
            />
          </div>
          <label>
            <DropdownInput value={type} onChange={handleChangeType}>
              {typeOptions.map((type) => (
                <OptionInput value={type}>
                  {toTitleCase(type ?? "")}
                </OptionInput>
              ))}
            </DropdownInput>
          </label>
        </div>
      );
    case JsonMultitypeSchemaWidgets.Checkbox:
      const otherType = schema.type.find((type) => type !== "null");
      return (
        <div
          className="form-spaced-group"
          style={{ display: "flex", alignItems: "center" }}
        >
          <CheckboxInput
            value={actualValue !== null}
            onChange={(value) =>
              setValue(
                value ? schemaDefault({ ...schema, type: otherType }) : null
              )
            }
          />
          <div style={{ width: "0.5em" }} />
          {actualValue !== null && (
            <SchemaBaseInput
              schema={{ ...schema, type: otherType }}
              error={error}
              value={actualValue}
              onChange={setValue}
              {...props}
            />
          )}
        </div>
      );
    default:
      return null;
  }
};

// Export React component and props.
export default SchemaMultitypeInput;
export type { SchemaMultitypeInputProps };
