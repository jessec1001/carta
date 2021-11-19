using CartaCore.Data;
using CartaCore.Integration.Hyperthought.Data;
using CartaCore.Operations.Hyperthought;
using CartaCore.Operations.Hyperthought.Data;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

namespace CartaTest.Operations
{
    /// <summary>
    /// Tests the <see cref="HyperthoughtProcessUpdateOperation"/> class helper methods
    /// </summary>
    [TestFixture]
    public class UpdateHyperthoughtWorkflowMetaData
    {

        /// <summary>
        /// Test update with null properties - should not throw null pointer
        /// </summary>
        [Test]
        public void TestNull()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(null, process);
            Assert.IsTrue(metaDataList.Count == 0);
        }

        /// <summary>
        /// Test update with a single property and no sub-properties
        /// </summary>
        [Test]
        public void TestPropertyOnly()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            List<HyperthoughtProperty> properties = new();
            properties.Add(new HyperthoughtProperty { Key = "test_property", Value = "test_value"});
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata metadata in metaDataList)
            {
                Assert.AreEqual("test_property", metadata.Key);
                HyperthoughtMetadataValue value = metadata.Value;
                Assert.AreEqual("test_value", metadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, metadata.Value.Type);
            }
        }

        /// <summary>
        /// Test update with a single property and a single sub-property
        /// </summary>
        [Test]
        public void TestPropertySubproperty()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "test_property", Value = "test_value" };
            property.Unit = "cm";
            properties.Add(property);

            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata metadata in metaDataList)
            {
                Assert.AreEqual("test_property", metadata.Key);
                HyperthoughtMetadataValue value = metadata.Value;
                Assert.AreEqual("test_value", metadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, metadata.Value.Type);

                Assert.IsTrue(metadata.Extensions.Count == 1);
                Assert.IsTrue(metadata.Extensions.ContainsKey("unit"));
                Assert.AreEqual("cm", metadata.Extensions["unit"]);
                Assert.IsNull(metadata.Annotation);
            }
        }

        /// <summary>
        /// Test update with a single property and multiple sub-properties
        /// </summary>
        [Test]
        public void TestPropertyManysubproperties()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "test_property", Value = "test_value" };
            property.Unit = "cm";
            property.Annotation = "test_annotation";
            properties.Add(property);
 
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata metadata in metaDataList)
            {
                Assert.AreEqual("test_property", metadata.Key);
                HyperthoughtMetadataValue value = metadata.Value;
                Assert.AreEqual("test_value", metadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, metadata.Value.Type);

                Assert.IsTrue(metadata.Extensions.Count == 1);
                Assert.IsTrue(metadata.Extensions.ContainsKey("unit"));
                Assert.AreEqual("cm", metadata.Extensions["unit"]);
                Assert.AreEqual("test_annotation", metadata.Annotation);
            }
        }

        /// <summary>
        /// Test update with multiple properties
        /// </summary>
        [Test]
        public void TestManyProperties()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "test_property", Value = "test_value" };
            property.Annotation = "test_annotation";
            properties.Add(property);
            HyperthoughtProperty property2 = new HyperthoughtProperty { Key = "test_property2", Value = "test_value2" };
            properties.Add(property2);

            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 2);
            int count = 0;
            foreach (HyperthoughtMetadata metadata in metaDataList)
            {
                count++;
                if (count == 1)
                {
                    Assert.AreEqual("test_property", metadata.Key);
                    HyperthoughtMetadataValue value = metadata.Value;
                    Assert.AreEqual("test_value", metadata.Value.Link);
                    Assert.AreEqual(HyperthoughtDataType.String, metadata.Value.Type);
                    Assert.AreEqual("test_annotation", metadata.Annotation);
                    Assert.IsNull(metadata.Extensions);
                }
                if (count == 2)
                {
                    Assert.AreEqual("test_property2", metadata.Key);
                    HyperthoughtMetadataValue value = metadata.Value;
                    Assert.AreEqual("test_value2", metadata.Value.Link);
                    Assert.AreEqual(HyperthoughtDataType.String, metadata.Value.Type);
                    Assert.IsNull(metadata.Annotation);
                    Assert.IsNull(metadata.Extensions);
                }
            }
        }

        /// <summary>
        /// Test that existing property does not get overwritten if no properties are specified
        /// </summary>
        [Test]
        public void TestExistingPropertyRemains()
        {
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProcess process = new HyperthoughtProcess();
            process.Metadata = new();
            HyperthoughtMetadata metadata = new HyperthoughtMetadata();
            metadata.Key = "existing_key";
            metadata.Value = new HyperthoughtMetadataValue();
            metadata.Value.Link = "existing_value";
            metadata.Value.Type = HyperthoughtDataType.String;
            process.Metadata.Add(metadata);
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata updatedMetadata in metaDataList)
            {
                Assert.AreEqual("existing_key", updatedMetadata.Key);
                HyperthoughtMetadataValue value = updatedMetadata.Value;
                Assert.AreEqual("existing_value", updatedMetadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);
            }
        }

        /// <summary>
        /// Test that existing property does not get overwritten if new properties are sepcified
        /// </summary>
        [Test]
        public void TestExistingPropertyRemainsWhenPropertiesAdded()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            process.Metadata = new();
            HyperthoughtMetadata metadata = new HyperthoughtMetadata();
            metadata.Key = "existing_key";
            metadata.Value = new HyperthoughtMetadataValue();
            metadata.Value.Link = "existing_value";
            metadata.Value.Type = HyperthoughtDataType.String;
            process.Metadata.Add(metadata);
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "new_key", Value = "new_value" };
            properties.Add(property);
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 2);
            int count = 0;
            foreach (HyperthoughtMetadata updatedMetadata in metaDataList)
            {
                count++;
                if (count == 1)
                {
                    Assert.AreEqual("existing_key", updatedMetadata.Key);
                    HyperthoughtMetadataValue value = updatedMetadata.Value;
                    Assert.AreEqual("existing_value", updatedMetadata.Value.Link);
                    Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);
                }
                if (count == 2)
                {
                    Assert.AreEqual("new_key", updatedMetadata.Key);
                    HyperthoughtMetadataValue value = updatedMetadata.Value;
                    Assert.AreEqual("new_value", updatedMetadata.Value.Link);
                    Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);
                }
            }
        }

        /// <summary>
        /// Test that existing property does get overwritten 
        /// </summary>
        [Test]
        public void TestExistingPropertyUpdate()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            process.Metadata = new();
            HyperthoughtMetadata metadata = new HyperthoughtMetadata();
            metadata.Key = "existing_key";
            metadata.Value = new HyperthoughtMetadataValue();
            metadata.Value.Link = "existing_value";
            metadata.Value.Type = HyperthoughtDataType.String;
            process.Metadata.Add(metadata);
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "existing_key", Value = "new_value" };
            properties.Add(property);
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata updatedMetadata in metaDataList)
            {
                Assert.AreEqual("existing_key", updatedMetadata.Key);
                HyperthoughtMetadataValue value = updatedMetadata.Value;
                Assert.AreEqual("new_value", updatedMetadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);
            }
        }

        /// <summary>
        /// Test that existing property does get overwritten and sub properties are added
        /// </summary>
        [Test]
        public void TestExistingPropertyUpdateAddSubProperties()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            process.Metadata = new();
            HyperthoughtMetadata metadata = new HyperthoughtMetadata();
            metadata.Key = "existing_key";
            metadata.Value = new HyperthoughtMetadataValue();
            metadata.Value.Link = "existing_value";
            metadata.Value.Type = HyperthoughtDataType.String;
            process.Metadata.Add(metadata);
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "existing_key", Value = "new_value" };
            property.Unit = "mm";
            property.Annotation = "test_annotation";
            properties.Add(property);
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata updatedMetadata in metaDataList)
            {
                Assert.AreEqual("existing_key", updatedMetadata.Key);
                HyperthoughtMetadataValue value = updatedMetadata.Value;
                Assert.AreEqual("new_value", updatedMetadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);

                Assert.IsTrue(metadata.Extensions.Count == 1);
                Assert.IsTrue(metadata.Extensions.ContainsKey("unit"));
                Assert.AreEqual("mm", metadata.Extensions["unit"]);
                Assert.AreEqual("test_annotation", metadata.Annotation);
            }
        }

        /// <summary>
        /// Test that existing property does get overwritten but existing sub properties are still intact, when
        /// new sub-properties are not specified
        /// </summary>
        [Test]
        public void TestExistingPropertyUpdateKeepSubProperties()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            process.Metadata = new();
            HyperthoughtMetadata metadata = new HyperthoughtMetadata();
            metadata.Key = "existing_key";
            metadata.Value = new HyperthoughtMetadataValue();
            metadata.Value.Link = "existing_value";
            metadata.Value.Type = HyperthoughtDataType.String;
            metadata.Extensions = new();
            metadata.Extensions.Add("unit", "m");
            metadata.Annotation = "test_annotation";
            process.Metadata.Add(metadata);
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "existing_key", Value = "new_value" };
            properties.Add(property);
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata updatedMetadata in metaDataList)
            {
                Assert.AreEqual("existing_key", updatedMetadata.Key);
                HyperthoughtMetadataValue value = updatedMetadata.Value;
                Assert.AreEqual("new_value", updatedMetadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);

                Assert.IsTrue(metadata.Extensions.Count == 1);
                Assert.IsTrue(metadata.Extensions.ContainsKey("unit"));
                Assert.AreEqual("m", metadata.Extensions["unit"]);
                Assert.AreEqual("test_annotation", metadata.Annotation);
            }
        }

        /// <summary>
        /// Test that existing property does not get overwritten when only new sub-properties are specified
        /// </summary>
        [Test]
        public void TestExistingPropertyRemainNewSubProperties()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            process.Metadata = new();
            HyperthoughtMetadata metadata = new HyperthoughtMetadata();
            metadata.Key = "existing_key";
            metadata.Value = new HyperthoughtMetadataValue();
            metadata.Value.Link = "existing_value";
            metadata.Value.Type = HyperthoughtDataType.String;
            process.Metadata.Add(metadata);
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "existing_key" };
            property.Unit = "cm";
            property.Annotation = "test_annotation";
            properties.Add(property);
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata updatedMetadata in metaDataList)
            {
                Assert.AreEqual("existing_key", updatedMetadata.Key);
                HyperthoughtMetadataValue value = updatedMetadata.Value;
                Assert.AreEqual("existing_value", updatedMetadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);

                Assert.IsTrue(metadata.Extensions.Count == 1);
                Assert.IsTrue(metadata.Extensions.ContainsKey("unit"));
                Assert.AreEqual("cm", metadata.Extensions["unit"]);
                Assert.AreEqual("test_annotation", metadata.Annotation);
            }
        }

        /// <summary>
        /// Test that existing sub-property gets overwritten
        /// </summary>
        [Test]
        public void TestExistingSubPropertyUpdated()
        {
            HyperthoughtProcess process = new HyperthoughtProcess();
            process.Metadata = new();
            HyperthoughtMetadata metadata = new HyperthoughtMetadata();
            metadata.Key = "existing_key";
            metadata.Value = new HyperthoughtMetadataValue();
            metadata.Value.Link = "existing_value";
            metadata.Value.Type = HyperthoughtDataType.String;
            metadata.Extensions = new();
            metadata.Extensions.Add("unit", "m");
            metadata.Extensions.Add("other_extension", "other");
            metadata.Annotation = "test_annotation";
            process.Metadata.Add(metadata);
            List<HyperthoughtProperty> properties = new();
            HyperthoughtProperty property = new HyperthoughtProperty { Key = "existing_key" };
            property.Unit = "cm";
            property.Annotation = "new_annotation";
            properties.Add(property);
            List<HyperthoughtMetadata> metaDataList =
                HyperthoughtProcessUpdateOperation.UpdateHyperthoughtMetaData(properties, process);
            Assert.IsTrue(metaDataList.Count == 1);
            foreach (HyperthoughtMetadata updatedMetadata in metaDataList)
            {
                Assert.AreEqual("existing_key", updatedMetadata.Key);
                HyperthoughtMetadataValue value = updatedMetadata.Value;
                Assert.AreEqual("existing_value", updatedMetadata.Value.Link);
                Assert.AreEqual(HyperthoughtDataType.String, updatedMetadata.Value.Type);

                Assert.IsTrue(metadata.Extensions.Count == 2);
                Assert.IsTrue(metadata.Extensions.ContainsKey("unit"));
                Assert.AreEqual("cm", metadata.Extensions["unit"]);
                Assert.AreEqual("new_annotation", metadata.Annotation);
            }
        }

    }
}
