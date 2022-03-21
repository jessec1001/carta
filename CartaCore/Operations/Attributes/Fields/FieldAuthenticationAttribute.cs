using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NJsonSchema;

namespace CartaCore.Operations.Attributes
{
    /// <summary>
    /// An attribute that specifies that a field should be filled in using the authentication system.
    /// The authentication type is used to initialize the field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FieldAuthenticationAttribute :
        Attribute,
        ISchemaModifierAttribute,
        IOverrideConversionAttribute
    {
        /// <summary>
        /// The type of structure that should be used for authentication.
        /// For instance, this may be <see cref="string" /> for an authentication token or a more complex structure
        /// containing a username or password for a database connection.
        /// </summary>
        public Type AuthenticationType { get; init; }
        /// <summary>
        /// The key that should be used when receiving authentication information from a client.
        /// /// </summary>
        public string Key { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAuthenticationAttribute"/> class.
        /// </summary>
        /// <param name="key">The key that should be used when receiving authentication information.</param>
        /// <param name="authType">The type of the authentication information structure that should be used.</param>
        public FieldAuthenticationAttribute(string key, Type authType)
        {
            Key = key;
            AuthenticationType = authType;
        }

        /// <inheritdoc />
        public async Task<object> ConvertInputField(
            Operation operation,
            OperationFieldDescriptor field,
            object input,
            OperationJob job)
        {
            // We ignore the input that was actually passed in and replace it with a corresponding authentication value.
            // If the authentication value was not provided, we add an entry to the tasks and trigger a job update.
            OperationJob root = job.Root;
            // TODO: For now this check is wrong, we should get the current operation ID rather than the root operation ID.
            if (
                root.Authentication.TryGetValue(root.Operation.Id, out ConcurrentDictionary<string, object> auth) &&
                auth.TryGetValue(Key, out object value)
            ) return await operation.ConvertInputField(field, value, job);
            else
            {
                root.Tasks.Add(new OperationTask()
                {
                    Operation = operation.Id,
                    Field = field.Name,
                    Type = OperationTaskType.Authenticate
                });
                return Activator.CreateInstance(AuthenticationType);
            }
        }
        /// <inheritdoc />
        public async Task<object> ConvertOutputField(
            Operation operation,
            OperationFieldDescriptor field,
            object output,
            OperationJob job)
        {
            // For now, this should not be specified as an output field.
            // Thus, we just perform the default functionality in this case.
            return await operation.ConvertOutputField(field, output, job);
        }

        /// <inheritdoc />
        public JsonSchema ModifySchema(JsonSchema schema)
        {
            // Generate a new schema for the authentication type itself.
            schema = OperationHelper.GenerateSchema(AuthenticationType);
            schema.Format = "hidden";
            return schema;
        }
    }
}