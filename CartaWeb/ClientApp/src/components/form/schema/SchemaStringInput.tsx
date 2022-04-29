import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import {
  schemaDefault,
  JsonStringSchema,
  JsonStringSchemaWidgets,
} from "library/schema";
import { TextFieldInput, TextAreaInput, ResourceInput } from "components/input";
import { SchemaTypedInputProps } from "./SchemaBaseInput";

/** The props used for the {@link SchemaStringInput} component. */
type SchemaStringInputProps = SchemaTypedInputProps<
  JsonStringSchema,
  string,
  JsonStringSchemaWidgets
>;

/** A component that inputs a string based on a schema as a field or area input. */
const SchemaStringInput: FunctionComponent<SchemaStringInputProps> = ({
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

  // By default, we should display this input as a text field.
  let uiWidget: JsonStringSchemaWidgets =
    widget ?? JsonStringSchemaWidgets.Field;
  if (schema["ui:widget"] !== undefined) uiWidget = schema["ui:widget"];

  // Check if there is a placeholder to display.
  const uiPlaceholder: string = schema["ui:placeholder"] ?? "";

  // Display based on the decided upon widget.
  switch (uiWidget) {
    case JsonStringSchemaWidgets.Field:
      return (
        <TextFieldInput
          value={actualValue}
          onChange={setValue}
          placeholder={uiPlaceholder}
          className={error === undefined ? undefined : "error"}
          {...props}
        />
      );
    case JsonStringSchemaWidgets.Area:
      return (
        <TextAreaInput
          value={actualValue}
          onChange={setValue}
          placeholder={uiPlaceholder}
          className={error === undefined ? undefined : "error"}
          {...props}
        />
      );
    case JsonStringSchemaWidgets.DataResource:
      return (
        <ResourceInput
          value={actualValue}
          filter={schema["ui:filter"]}
          onChange={setValue}
          className={error === undefined ? undefined : "error"}
          {...props}
        />
      );
    default:
      return null;
  }
};

// Export React component and props.
export default SchemaStringInput;
export type { SchemaStringInputProps };
