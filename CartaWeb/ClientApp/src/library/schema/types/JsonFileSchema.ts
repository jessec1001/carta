import { JsonBaseTypedSchema } from "..";

/** The type of widgets that can be used to input a file. */
enum JsonFileSchemaWidgets {
  DragDrop = "dragdrop",
}

/** The schema specification for a JSON file. */
interface JsonFileSchema extends JsonBaseTypedSchema<"file", any> {
  multiple?: boolean;
  minLength?: number;
  maxLength?: number;
}

export default JsonFileSchema;
export { JsonFileSchemaWidgets };
