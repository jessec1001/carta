import { Component, FormEvent, HTMLProps } from "react";
import JsonSchema, { JsonSchemaType } from "library/schema/JsonSchema";
import { SchemaBaseFormProps } from "./SchemaBaseForm";
import SchemaNumberForm from "./SchemaNumberForm";
import SchemaStringForm from "./SchemaStringForm";
import SchemaBooleanForm from "./SchemaBooleanForm";
import SchemaEnumForm from "./SchemaEnumForm";
import SchemaObjectForm from "./SchemaObjectForm";
import SchemaArrayForm from "./SchemaArrayForm";

/** The props used for the {@link SchemaForm} component. */
interface SchemaFormProps extends HTMLProps<HTMLFormElement> {
  schema: JsonSchema;
  value?: any;

  onChange?: (value: any) => void;
  onSubmit?: (value: any) => void;
}

/** A form component that inputs a value according to a JSON schema. */
class SchemaForm extends Component<SchemaFormProps> {
  constructor(props: SchemaFormProps) {
    super(props);

    // Bind event handlers.
    this.handleSubmit = this.handleSubmit.bind(this);
    this.handleChange = this.handleChange.bind(this);
  }

  /** Handles submitting the form. */
  private handleSubmit(event: FormEvent<HTMLFormElement>) {
    const { value } = this.props;
    const { onSubmit } = this.props;
    if (onSubmit) {
      onSubmit(value);
      event.preventDefault();
    }
  }
  /** Handles changing the form value. */
  private handleChange(value: any) {
    const { onChange } = this.props;
    if (onChange) onChange(value);
  }

  render() {
    const { schema, value, ...restProps } = this.props;
    const { onChange, onSubmit, ...formProps } = restProps;
    return (
      <form {...formProps} onSubmit={this.handleSubmit}>
        {SchemaForm.renderProperty({
          schema: schema,
          value: value,
          onChange: this.handleChange,
        })}
        <input type="submit" />
      </form>
    );
  }

  /** Renders a specific form based on its schema type. */
  static renderProperty<T extends JsonSchemaType>(
    props: SchemaBaseFormProps<T, any>
  ): JSX.Element {
    switch (props.schema.type) {
      case "object":
        return <SchemaObjectForm {...(props as any)} />;
      case "array":
        return <SchemaArrayForm {...(props as any)} />;
      case "integer":
      case "number":
        return <SchemaNumberForm {...(props as any)} />;
      case "boolean":
        return <SchemaBooleanForm {...(props as any)} />;
      case "string":
      case undefined:
        if ("enum" in props.schema)
          return <SchemaEnumForm {...(props as any)} />;
        else return <SchemaStringForm {...(props as any)} />;
    }
    return null as any;
  }
}

// Export component and underlying types.
export default SchemaForm;
export type { SchemaFormProps };
