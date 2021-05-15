import { JsonSchemaObject } from "library/schema";
import SchemaBaseForm, { SchemaBaseFormProps } from "./SchemaBaseForm";
import SchemaForm from "./SchemaForm";

/** A form that inputs an object according to a JSON schema. */
class SchemaObjectForm extends SchemaBaseForm<JsonSchemaObject, any> {
  constructor(props: SchemaBaseFormProps<JsonSchemaObject, any>) {
    super(props);

    // Bind event handlers.
    this.handlePropertyChange = this.handlePropertyChange.bind(this);
  }

  protected defaultValue() {
    return {};
  }
  protected renderInner() {
    // Render the properties of the object as new individual forms.
    const { schema, value } = this.props;
    const properties = schema.properties ?? {};
    return (
      <fieldset>
        {value !== undefined &&
          Object.entries(properties).map(([prop, schema]) => {
            return SchemaForm.renderProperty({
              schema: { title: prop, ...schema },
              value: value[prop],
              onChange: (propValue) =>
                this.handlePropertyChange(prop, propValue),
            });
          })}
      </fieldset>
    );
  }

  /** Handles changes to individual properties of the form. */
  private handlePropertyChange(prop: string, value: any) {
    const oldValue = this.props.value;
    const newValue = {
      ...oldValue,
      [prop]: value,
    };
    this.handleChange(newValue);
  }
}

// Export component.
export default SchemaObjectForm;
