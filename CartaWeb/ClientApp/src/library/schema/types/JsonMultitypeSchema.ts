import { JsonBaseSchema, JsonSchemaBasicTypename } from "../JsonBaseSchema";

enum JsonMultitypeSchemaWidgets {
  Dropdown = "dropdown",
}

interface JsonMultitypeSchema extends JsonBaseSchema {
  type: JsonSchemaBasicTypename[];

  "ui:widget"?: JsonMultitypeSchemaWidgets;
}

export default JsonMultitypeSchema;
export { JsonMultitypeSchemaWidgets };
