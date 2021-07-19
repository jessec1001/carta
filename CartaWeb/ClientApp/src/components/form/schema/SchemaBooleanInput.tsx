import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import {
  JsonBooleanSchema,
  JsonBooleanSchemaWidgets,
  schemaDefault,
} from "library/schema";
import { CheckboxInput } from "components/input";
import { SchemaTypedInputProps } from "./SchemaBaseInput";

/** The props used for the {@link SchemaBooleanInput} component. */
type SchemaBooleanInputProps = SchemaTypedInputProps<
  JsonBooleanSchema,
  boolean,
  JsonBooleanSchemaWidgets
>;

/** A component that inputs a boolean based on a schema as a checkbox. */
const SchemaBooleanInput: FunctionComponent<SchemaBooleanInputProps> = ({
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

  // By default, we should display this input as a checkbox.
  let uiWidget: JsonBooleanSchemaWidgets =
    widget ?? JsonBooleanSchemaWidgets.Checkbox;
  if (schema["ui:widget"] !== undefined) uiWidget = schema["ui:widget"];

  // Display based on the decided upon widget.
  switch (uiWidget) {
    case JsonBooleanSchemaWidgets.Checkbox:
      return (
        <CheckboxInput
          value={actualValue}
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
export default SchemaBooleanInput;
export type { SchemaBooleanInputProps };
