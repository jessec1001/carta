import { JsonBaseSchema } from "../JsonBaseSchema";

/** The type of widgets that can be used to input a JSON enumeration. */
enum JsonEnumSchemaWidgets {
  Dropdown = "dropdown",
}

/** The schema specification for a JSON enumeration value. */
interface JsonEnumSchema extends JsonBaseSchema {
  enum: any[];

  "ui:widget"?: JsonEnumSchemaWidgets;
}

export default JsonEnumSchema;
export { JsonEnumSchemaWidgets };
