import { JsonSchemaNumber } from "library/schema";
import SchemaBaseForm from "./SchemaBaseForm";

/** A form that inputs a number value according to a JSON schema. */
class SchemaNumberForm extends SchemaBaseForm<JsonSchemaNumber, number> {
  protected defaultValue() {
    // The default value should be, in order of being defined, the minimum, the maximum, and zero.
    const { schema } = this.props;
    if (schema.minimum !== undefined) return schema.minimum;
    if (schema.maximum !== undefined) return schema.maximum;
    return 0;
  }
  protected renderInner() {
    // If one of the min or max of the range is undefined, we should display this input as a number field.
    // Otherwise, we should display this input as a number range.
    const { schema } = this.props;
    if (schema.minimum === undefined || schema.maximum === undefined)
      return this.renderField();
    else return this.renderRange();
  }

  /** Computes the step size of the number. */
  private computeStep() {
    const { schema } = this.props;
    if (schema.multipleOf === undefined) {
      if (schema.type === "integer") return 1;
      else if (schema.type === "number") return "any";
    } else return schema.multipleOf;
  }

  /** Renders the inner input as a text field-like input. */
  private renderField() {
    const { schema, value } = this.props;
    return (
      <input
        type="number"
        min={schema.minimum}
        max={schema.maximum}
        step={this.computeStep()}
        value={value}
        onChange={(event) => this.handleChange(event.target.valueAsNumber)}
      />
    );
  }
  /** Renders the inner input as a slider-like input. */
  private renderRange() {
    const { schema, value } = this.props;
    return (
      <input
        type="range"
        min={schema.minimum}
        max={schema.maximum}
        step={this.computeStep()}
        value={value}
        onChange={(event) => this.handleChange(event.target.valueAsNumber)}
      />
    );
  }
}

// Export component.
export default SchemaNumberForm;
