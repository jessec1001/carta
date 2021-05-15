import { JsonSchemaArray } from "library/schema";
import SchemaBaseForm, { SchemaBaseFormProps } from "./SchemaBaseForm";
import SchemaForm from "./SchemaForm";

class SchemaArrayForm extends SchemaBaseForm<JsonSchemaArray, any[]> {
  constructor(props: SchemaBaseFormProps<JsonSchemaArray, any[]>) {
    super(props);

    // Bind event handlers.
    this.handleItemChange = this.handleItemChange.bind(this);
    this.handleItemAdd = this.handleItemAdd.bind(this);
    this.handleItemRemove = this.handleItemRemove.bind(this);
  }

  protected defaultValue() {
    return [];
  }
  protected renderInner() {
    // Render each of the items as their own forms in a list.
    // Also render a button to add new items if the max items hasn't been met yet.
    const { value, schema } = this.props;
    const canAdd =
      schema.maxItems === undefined || schema.maxItems > (value?.length ?? 0);
    return (
      <fieldset>
        {value?.map((subvalue, index) => {
          const subschema = this.computeSubschema(index);
          if (subschema) {
            return (
              <div>
                {SchemaForm.renderProperty({
                  schema: subschema,
                  value: subvalue,
                  onChange: (itemValue) =>
                    this.handleItemChange(index, itemValue),
                })}
                {canAdd && (
                  <button
                    onClick={(event) => {
                      this.handleItemRemove(index);
                      event.preventDefault();
                    }}
                  >
                    Delete
                  </button>
                )}
              </div>
            );
          } else return false;
        })}
        <button
          onClick={(event) => {
            this.handleItemAdd();
            event.preventDefault();
          }}
        >
          Add
        </button>
      </fieldset>
    );
  }

  /** Computes the schema of particular element of the array. */
  private computeSubschema(index: number) {
    // Get the schema of a particular element of list depending on
    const { schema } = this.props;
    if (Array.isArray(schema.items)) {
      if (schema.items.length < index) return schema.items[index];
      else return null;
    } else if (schema.items) {
      return schema.items;
    } else return null;
  }

  /** Handles changes to individual items of the form. */
  private handleItemChange(index: number, value: any) {
    // Make sure that the value is defined before handling this method.
    if (this.props.value === undefined) return;

    // We change the particular value in the array immutably.
    const oldValue = this.props.value;
    const newValue = [...oldValue];
    newValue[index] = value;

    // Notify the container element of the change.
    const { onChange } = this.props;
    if (onChange) onChange(newValue);
  }
  /** Handles adding a new item to the form. */
  private handleItemAdd() {
    // Make sure that the value is defined before handling this method.
    if (this.props.value === undefined) return;

    // We append a new blank element to the array immutably.
    const oldValue = this.props.value;
    const newValue = [...oldValue];
    newValue.push(undefined);

    // Notify the container element of the change.
    const { onChange } = this.props;
    if (onChange) onChange(newValue);
  }
  /** Handles removing an existing item from the form. */
  private handleItemRemove(index: number) {
    // Make sure that the value is defined before handling this method.
    if (this.props.value === undefined) return;

    // We remove the element at the specified index from the array immutably.
    const oldValue = this.props.value;
    const newValue = [...oldValue];
    newValue.splice(index, 1);

    // Notify the container element of the change.
    const { onChange } = this.props;
    if (onChange) onChange(newValue);
  }
}

// Export component.
export default SchemaArrayForm;
