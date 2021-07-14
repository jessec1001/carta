import { JsonBaseTypedSchema } from "../JsonBaseSchema";

/** The type of widgets that can be used to input a JSON null value. */
enum JsonNullSchemaWidgets {}

/** The schema specification for a JSON null value. */
interface JsonNullSchema extends JsonBaseTypedSchema<"null", null> {}

export default JsonNullSchema;
export { JsonNullSchemaWidgets };
