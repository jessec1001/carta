import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import {
  JsonEnumSchema,
  JsonEnumSchemaWidgets,
  schemaDefault,
} from "library/schema";
import { DropdownInput, OptionInput } from "components/input";
import { SchemaTypedInputProps } from "./SchemaBaseInput";

/** The props used for the {@link SchemaEnumInput} component. */
type SchemaEnumInputProps = SchemaTypedInputProps<
  JsonEnumSchema,
  any,
  JsonEnumSchemaWidgets
>;

/** A component that inputs a value based on an enumeration schema as a dropdown input. */
const SchemaEnumInput: FunctionComponent<SchemaEnumInputProps> = ({
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

  // By default, we should display this input as a dropdown.
  let uiWidget: JsonEnumSchemaWidgets =
    widget ?? JsonEnumSchemaWidgets.Dropdown;
  if (schema["ui:widget"] !== undefined) uiWidget = schema["ui:widget"];

  // We check if there are names for the enumeration values.
  const enumNames = schema["x-enumNames"];

  // FIXME: Warning is thrown because dropdown input ends up setting state in this component.
  // Display based on the decided upon widget.
  switch (uiWidget) {
    case JsonEnumSchemaWidgets.Dropdown:
      return (
        <DropdownInput
          value={actualValue}
          onChange={setValue}
          className={error === undefined ? undefined : "error"}
          {...props}
        >
          {schema.enum.map((option, index) => (
            <OptionInput value={option}>
              {enumNames ? enumNames[index] : option}
            </OptionInput>
          ))}
        </DropdownInput>
      );
    default:
      return null;
  }
};

// Export React component and props.
export default SchemaEnumInput;
export type { SchemaEnumInputProps };
