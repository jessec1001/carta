import { FunctionComponent } from "react";
import { useControllableState } from "hooks";
import classNames from "classnames";
import {
  JsonObjectSchema,
  JsonObjectSchemaWidgets,
  schemaDefault,
  ValidationError,
} from "library/schema";
import FormGroup from "../FormGroup";
import SchemaBaseInput, { SchemaTypedInputProps } from "./SchemaBaseInput";

/** The props used for the {@link SchemaObjectInput} component. */
type SchemaObjectInputProps = SchemaTypedInputProps<
  JsonObjectSchema,
  any,
  JsonObjectSchemaWidgets
>;

/** A component that inputs an object with properties based on a schema as a list. */
const SchemaObjectInput: FunctionComponent<SchemaObjectInputProps> = ({
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

  // Create event handlers.
  // This event handler updates the object through a single property change that is shallowly merged in.
  const handleChangeProperty = (property: string, value: any) => {
    const nextValue = { ...actualValue, [property]: value };
    setValue(nextValue);
  };

  // By default, we should display this input as a list.
  let uiWidget: JsonObjectSchemaWidgets =
    widget ?? JsonObjectSchemaWidgets.List;
  if (schema["ui:widget"] !== undefined) uiWidget = schema["ui:widget"];

  // Extract the relevant properties of the schema.
  const properties = schema.properties ?? {};

  // Display based on the decided upon widget.
  switch (uiWidget) {
    case JsonObjectSchemaWidgets.List:
      if (Object.keys(properties).length === 0) return null;

      return (
        <div className={classNames("form-container", { error })}>
          {Object.entries(properties).map(([property, subschema]) => {
            // Deconstruct error and create suberror if necessary.
            let suberror: ValidationError | undefined = undefined;
            let suberrorKey: string | undefined = undefined;
            if (error !== undefined) {
              const suberrorTrace = [...error.trace];
              suberrorKey =
                error === undefined
                  ? undefined
                  : (suberrorTrace.shift() as string | undefined);
              suberror = new ValidationError(suberrorTrace, error.message);
            }

            // We use the base input to render each property with its correct subschema.
            return (
              <FormGroup
                error={
                  suberrorKey === property && suberror?.trace?.length === 0
                    ? suberror
                    : undefined
                }
                title={subschema.title}
                description={subschema.description}
                key={property}
              >
                <SchemaBaseInput
                  schema={subschema}
                  error={suberrorKey === property ? suberror : undefined}
                  value={actualValue[property]}
                  onChange={(value) => handleChangeProperty(property, value)}
                  {...props}
                />
              </FormGroup>
            );
          })}
        </div>
      );
    default:
      return null;
  }
};

// Export React component and props.
export default SchemaObjectInput;
export type { SchemaObjectInputProps };
