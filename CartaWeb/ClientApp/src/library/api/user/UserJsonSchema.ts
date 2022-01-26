import {
  JsonObjectSchemaWidgets,
  JsonSchema,
  JsonStringSchema,
} from "library/schema";

/** A {@link JsonSchema} for a user object. */
const UserJsonSchema: JsonSchema = {
  type: "object",
  properties: {
    userInformation: {
      type: "object",
      properties: {
        id: {
          type: "string",
          minLength: 1,
        } as JsonStringSchema,
        name: {
          type: "string",
        } as JsonStringSchema,
      },
    },
    required: ["id", "name"],
  },
  required: ["userInformation"],
  "ui:widget": JsonObjectSchemaWidgets.User,
} as JsonSchema;

export default UserJsonSchema;
