using System.Collections.Generic;
using System.Text.Json.Serialization;
using CartaCore.Operations;
using CartaCore.Persistence;


namespace CartaWeb.Models.DocumentItem
{
    public class TestItem : Item
    {
        public TestItem() { }

        public TestItem(string secretId, string testId)
            : base(testId, secretId) { }

        [Secret]
        public Dictionary<string, object> SecretValues { get; set; }

        public string SomeOtherProperty { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public override string PartitionKeyPrefix => "TEST#";
        /// <inheritdoc />
        [JsonIgnore]
        public override string SortKeyPrefix => "SECRET#";
    }
}
