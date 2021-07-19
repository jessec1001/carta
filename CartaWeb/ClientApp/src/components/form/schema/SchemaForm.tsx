import React, {
  FunctionComponent,
  HTMLProps,
  useState,
} from "react";
import { useControllableState } from "hooks";
import { Modify } from "types";
import {
  flattenSchema,
  JsonSchema,
  schemaDefault,
  validateSchema,
  ValidationError,
} from "library/schema";
import Logging, { LogSeverity } from "library/logging";
import { BlockButton } from "components/ui/buttons";
import FormGroup from "../FormGroup";
import SchemaBaseInput from "./SchemaBaseInput";

import "../form.css";

/** The props used for the {@link SchemaForm} component. */
interface SchemaFormProps {
  schema: JsonSchema;
  value?: any;

  cancelable?: boolean;

  submitText?: string;
  cancelText?: string;

  onChange?: (value: any) => void;
  onSubmit?: (value: any) => void;
  onError?: (error: ValidationError) => void;
  onCancel?: () => void;
}

/** A form component that inputs a value according to a JSON schema. */
const SchemaForm: FunctionComponent<
  Modify<HTMLProps<HTMLFormElement>, SchemaFormProps>
> = ({
  schema,
  value,
  cancelable,
  submitText,
  cancelText,
  onChange,
  onSubmit,
  onError,
  onCancel,
}) => {
  // We allow the component to have an optionally controlled value.
  // Notice that we flatten the schema first so that our interactive components are simpler.
  const actualSchema = flattenSchema(schema);
  const defaultValue = schemaDefault(actualSchema, value);
  const [actualValue, setValue] = useControllableState(
    defaultValue,
    defaultValue,
    onChange
  );

  // We use an internal state only for error handling.
  const [error, setError] = useState<ValidationError | undefined>(undefined);

  // Set defaults for the button text.
  submitText = submitText ?? "Submit";
  cancelText = cancelText ?? "Cancel";

  // We forward cancellations.
  const handleCancel = () => {
    Logging.log({
      severity: LogSeverity.Debug,
      source: "Schema Form",
      title: "Canceled Form",
    });
    if (onCancel) onCancel();
  };
  // We forward value changes.
  const handleChange = (value: any) => {
    // We clear errors when we change any inputs.
    setError(undefined);
    setValue(value);
  };
  // We stop the normal submit functionality of a form since this is a SPA.
  const handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
    // Try validating the submitted value against the schema.
    // The validation error variable will be set if the validation fails.
    let validationError: ValidationError | undefined = undefined;
    try {
      validateSchema(actualSchema, actualValue);
    } catch (err) {
      if (err instanceof ValidationError) {
        validationError = err;
      } else throw err;
    }
    setError(validationError);

    // Try submitting if we validated successfully.
    // Try erroring if we validated unsuccessfully.
    if (validationError === undefined && onSubmit) {
      Logging.log({
        severity: LogSeverity.Debug,
        source: "Schema Form",
        title: "Submitted Form",
        data: actualValue,
      });
      onSubmit(actualValue);
    }
    if (validationError !== undefined && onError) {
      Logging.log({
        severity: LogSeverity.Debug,
        source: "Schema Form",
        title: "Errored Form",
        data: [actualValue, validationError],
      });
      onError(validationError);
    }
    event.preventDefault();
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* We wrap whatever input is required in an overall form group. */}
      <FormGroup
        error={error?.trace === [] ? error : undefined}
        title={actualSchema.title}
        description={actualSchema.description}
      >
        <SchemaBaseInput
          schema={actualSchema}
          error={error}
          value={actualValue}
          onChange={handleChange}
        />
      </FormGroup>

      {/* Submit and cancel buttons go down here. */}
      <div className="form-spaced-group">
        <BlockButton type="submit" color="primary">
          {submitText}
        </BlockButton>
        {cancelable && (
          <BlockButton color="secondary" onClick={handleCancel}>
            {cancelText}
          </BlockButton>
        )}
      </div>
    </form>
  );
};

// Export component and underlying types.
export default SchemaForm;
export type { SchemaFormProps };
