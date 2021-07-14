import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import {
  schemaDefault,
  JsonNumberSchema,
  JsonNumberSchemaWidgets,
} from "library/schema";
import { NumberFieldInput, NumberSliderInput } from "components/ui/form/input";
import { SchemaTypedInputProps } from "./SchemaBaseInput";

/** The props used for the {@link SchemaNumberInput} component. */
type SchemaNumberInputProps = SchemaTypedInputProps<
  JsonNumberSchema,
  number,
  JsonNumberSchemaWidgets
>;

/**
 * Computes the step (multiple of) for the UI component from the schema.
 * @param schema The number schema to compute step from.
 */
const computeStep = (schema: JsonNumberSchema) => {
  if (schema.multipleOf === undefined) {
    if (schema.type === "integer") return 1;
    else return "any";
  } else return schema.multipleOf;
};

const computeMinMax = (schema: JsonNumberSchema) => {
  if (schema.multipleOf === undefined) return [schema.minimum, schema.maximum];
  else {
    const minimum =
      schema.minimum === undefined
        ? undefined
        : schema.multipleOf * Math.ceil(schema.minimum / schema.multipleOf);
    const maximum =
      schema.maximum === undefined
        ? undefined
        : schema.multipleOf * Math.floor(schema.maximum / schema.multipleOf);
    return [minimum, maximum];
  }
};

/** A component that inputs a number based on a schema as a range or field input. */
const SchemaNumberInput: FunctionComponent<SchemaNumberInputProps> = ({
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

  // If one of the min or max of the range is undefined, we should display this input as a number field.
  // Otherwise, we should display this input as a number range.
  let uiWidget: JsonNumberSchemaWidgets =
    widget ?? JsonNumberSchemaWidgets.Field;
  if (widget === undefined) {
    if (schema.minimum !== undefined && schema.maximum !== undefined)
      uiWidget = JsonNumberSchemaWidgets.Slider;
  }
  if (schema["ui:widget"] !== undefined) uiWidget = schema["ui:widget"];

  // Determine the actual minimum and maximum based on multiple of.
  const [minimum, maximum] = computeMinMax(schema);

  // Display based on the decided upon widget.
  switch (uiWidget) {
    case JsonNumberSchemaWidgets.Field:
      return (
        <NumberFieldInput
          min={minimum}
          max={maximum}
          step={computeStep(schema)}
          value={actualValue}
          onChange={setValue}
          className={error === undefined ? undefined : "error"}
          {...props}
        />
      );
    case JsonNumberSchemaWidgets.Slider:
      return (
        <NumberSliderInput
          min={minimum}
          max={maximum}
          step={computeStep(schema)}
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
export default SchemaNumberInput;
export type { SchemaNumberInputProps };
