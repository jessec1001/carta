import { Component } from "react";
import { JsonSchemaType } from "library/schema";

/** The props used for the {@link SchemaBaseForm} component. */
interface SchemaBaseFormProps<TSchema extends JsonSchemaType, TValue> {
  schema: TSchema;
  value?: TValue;

  onChange?: (value: TValue) => void;
}

/** A abstract base component that basic schema form components implement. */
class SchemaBaseForm<TSchema extends JsonSchemaType, TValue> extends Component<
  SchemaBaseFormProps<TSchema, TValue>
> {
  constructor(props: SchemaBaseFormProps<TSchema, TValue>) {
    super(props);

    // Bind event handlers.
    this.handleChange = this.handleChange.bind(this);
  }

  componentDidMount() {
    // Retrieve the schema and value properties.
    const { schema, value } = this.props;
    const { onChange } = this.props;

    // If a value was not passed into the component, we need to try to change the value to some default by invoking the
    // parent 'change' event.
    if (value === undefined) {
      const defaultValue =
        schema.default === undefined
          ? this.defaultValue()
          : (schema.default as TValue);
      if (onChange) onChange(defaultValue);
    }
  }
  render() {
    // Render the wrapper around the form field and allow the base class to perform the rendering of the inner input
    // components.
    const { schema } = this.props;
    return (
      <div>
        <label>{schema.title}</label>
        <p>{schema.description}</p>
        {this.renderInner()}
      </div>
    );
  }

  /**
   * Handles changes to the value of the form.
   * @param event The change event from an input element.
   */
  protected handleChange(value: TValue) {
    const { onChange } = this.props;
    if (onChange) onChange(value);
  }

  /**
   * Gets the default value for the form value.
   */
  protected defaultValue(): TValue {
    throw new Error("Use of abstract base form.");
  }
  /**
   * Renders the inner input component of the form.
   */
  protected renderInner(): JSX.Element {
    throw new Error("Use of abstract base form.");
  }
}

// Export component and underlying types.
export default SchemaBaseForm;
export type { SchemaBaseFormProps };
