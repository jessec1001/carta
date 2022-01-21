import { JsonBaseSchema, JsonSchemaBasicTypename } from "../JsonBaseSchema";

enum JsonMultitypeSchemaWidgets {
  Dropdown = "dropdown",
  Checkbox = "checkbox",
}

interface JsonMultitypeSchema extends JsonBaseSchema {
  type: JsonSchemaBasicTypename[];

  "ui:widget"?: JsonMultitypeSchemaWidgets;
}

export default JsonMultitypeSchema;
export { JsonMultitypeSchemaWidgets };
