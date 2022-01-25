import { JsonFileSchema, JsonFileSchemaWidgets } from "library/schema";
import { FunctionComponent } from "react";
import { Text } from "components/text";
import { SchemaTypedInputProps } from "./SchemaBaseInput";
import { useControllableState } from "hooks";

/** The props used for the {@link SchemaFileInput} component. */
type SchemaFileInputProps = SchemaTypedInputProps<
  JsonFileSchema,
  File,
  JsonFileSchemaWidgets
>;

const SchemaFileInput: FunctionComponent<SchemaFileInputProps> = ({
  schema,
  widget,
  error,
  value,
  onChange,
  children,
  ...props
}) => {
  const [actualValue, setValue] = useControllableState(
    // TODO: Change files value to an array (FileList) that defaults to empty.
    () => undefined as unknown as File,
    value,
    onChange
  );

  // TODO: Consider converting to dedicated input component.
  return (
    <>
      <style>
        {`.input-file-text:hover {
                color: var(--color-primary);
            }`}
      </style>
      <div
        style={{
          padding: "1rem",
          border: "1px dashed var(--color-stroke-hairline)",
          backgroundImage: `url("upload.svg")`,
          backgroundSize: "20%",
          backgroundRepeat: "no-repeat",
          backgroundPosition: "90%",
        }}
        className="input-file"
      >
        <label>
          <span
            className="input-file-text"
            style={{
              cursor: "pointer",
            }}
          >
            <Text>Choose File</Text>
          </span>
          <div style={{ height: "0.25rem" }} />
          <Text color="muted" size="small">
            {actualValue && actualValue.name}
          </Text>
          <input
            style={{
              width: "0.1px",
              height: "0.1px",
              overflow: "hidden",
              opacity: 0,
              position: "absolute",
              zIndex: -1,
            }}
            type="file"
            onChange={(event) => {
              setValue(
                event.target.files?.length
                  ? event.target.files[0]
                  : (null as unknown as File)
              );
            }}
          />
        </label>
      </div>
    </>
  );
};

export default SchemaFileInput;
export type { SchemaFileInputProps };
