import { JsonSchemaString } from "library/schema";
import SchemaBaseForm from "./SchemaBaseForm";

/** A form that inputs a string value according to a JSON schema. */
class SchemaStringForm extends SchemaBaseForm<JsonSchemaString, string> {
  protected defaultValue() {
    return "";
  }
  protected renderInner() {
    const { value } = this.props;
    return (
      <input
        type="text"
        value={value}
        onChange={(event) => this.handleChange(event.target.value)}
      />
    );
  }
}

// Export component.
export default SchemaStringForm;
