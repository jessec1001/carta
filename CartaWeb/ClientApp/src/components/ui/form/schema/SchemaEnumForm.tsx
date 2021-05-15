import { JsonSchemaEnum } from "library/schema";
import SchemaBaseForm from "./SchemaBaseForm";

/** A form that inputs an enumeration value according to a JSON schema. */
class SchemaEnumForm extends SchemaBaseForm<JsonSchemaEnum, any> {
  protected defaultValue() {
    // The default value should be the first entry of the enumeration.
    const { schema } = this.props;
    if (schema.enum.length > 0) return schema.enum[0];
    return null;
  }
  protected renderInner() {
    // Render a dropdown using the values specified in the schema.
    const { schema, value } = this.props;
    return (
      <select
        value={value}
        onChange={(event) => this.handleChange(event.target.value)}
      >
        {schema.enum.map((value) => (
          <option value={value}>{value}</option>
        ))}
      </select>
    );
  }
}

// Export component.
export default SchemaEnumForm;
