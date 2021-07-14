import { JsonBaseTypedSchema } from "../JsonBaseSchema";

/** The type of widgets that can be used to input a JSON boolean. */
enum JsonBooleanSchemaWidgets {
  Checkbox = "checkbox",
}

/** The schema specification for a JSON boolean. */
interface JsonBooleanSchema extends JsonBaseTypedSchema<"boolean", boolean> {
  "ui:widget"?: JsonBooleanSchemaWidgets;
}

export default JsonBooleanSchema;
export { JsonBooleanSchemaWidgets };
