import { JsonBaseTypedSchema } from "../JsonBaseSchema";

/** The type of widgets that can be used to input a JSON string. */
enum JsonStringSchemaWidgets {
  Field = "field",
  Area = "area",
}

/** The schema specification for a JSON string. */
interface JsonStringSchema extends JsonBaseTypedSchema<"string", string> {
  minLength?: number;
  maxLength?: number;
  pattern?: string;
  format?: "regex";

  "ui:widget"?: JsonStringSchemaWidgets;
  "ui:placeholder"?: string;
}

export default JsonStringSchema;
export { JsonStringSchemaWidgets };
