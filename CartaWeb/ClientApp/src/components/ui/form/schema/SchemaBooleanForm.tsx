import { JsonSchemaBoolean } from "library/schema";
import SchemaBaseForm from "./SchemaBaseForm";

/** A form that inputs a boolean value according to a JSON schema. */
class SchemaBooleanForm extends SchemaBaseForm<JsonSchemaBoolean, boolean> {
  protected defaultValue() {
    return false;
  }
  protected renderInner() {
    const { value } = this.props;
    return (
      <input
        type="checkbox"
        checked={value}
        onChange={(event) => this.handleChange(event.target.checked)}
      />
    );
  }
}

// Export component.
export default SchemaBooleanForm;
