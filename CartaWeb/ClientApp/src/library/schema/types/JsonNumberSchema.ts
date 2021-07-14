import { JsonBaseTypedSchema } from "../JsonBaseSchema";

/** The type of widgets that can be used to input a JSON number. */
enum JsonNumberSchemaWidgets {
  Field = "field",
  Slider = "slider",
}

/** The schema specification for a JSON number. */
interface JsonNumberSchema
  extends JsonBaseTypedSchema<"integer" | "number", number> {
  multipleOf?: number;
  minimum?: number;
  maximum?: number;
  exclusiveMinimum?: boolean;
  exclusiveMaximum?: boolean;
  format?: "double";

  "ui:widget"?: JsonNumberSchemaWidgets;
}

export default JsonNumberSchema;
export { JsonNumberSchemaWidgets };
