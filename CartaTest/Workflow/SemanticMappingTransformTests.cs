using System;
using System.Collections.Generic;

using NUnit.Framework;

using CartaCore.Data;
using CartaCore.Workflow;

namespace CartaTest.Workflow
{
    /// <summary>
    /// Tests the <see cref="SemanticMappingTransform"/> class.
    /// </summary>
    [TestFixture]
    public class SemanticMappingTransformTests
    {
        /// <summary>
        /// The vertex to perform transformation tests on.
        /// </summary>
        public FreeformVertex Vertex;

        /// <summary>
        /// A semantic mapping transformation with some entries.
        /// </summary>
        public SemanticMappingTransform Transform;
        /// <summary>
        /// A semantic mapping transformation with no entries.
        /// </summary>
        public SemanticMappingTransform TransformEmpty;
        /// <summary>
        /// A semantic mapping transformation constructed with a null map.
        /// </summary>
        public SemanticMappingTransform TransformNull;

        /// <summary>
        /// Sets up the tests for semantic mapping transformations.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // Set the vertex instance.
            Vertex = new FreeformVertex
            {
                Id = Guid.NewGuid(),
                Properties = new SortedList<string, FreeformVertexProperty>
                (
                    new Dictionary<string, FreeformVertexProperty>()
                    {
                        ["foo"] = new FreeformVertexProperty { Value = 1, Type = typeof(int) },
                        ["baz"] = new FreeformVertexProperty { Value = 2, Type = typeof(int) },
                        ["bar"] = new FreeformVertexProperty { Value = 3, Type = typeof(int) },
                        ["qux"] = new FreeformVertexProperty { Value = 4, Type = typeof(int) }
                    }
                )
            };

            // Set the semantic mapping instances.
            Transform = new SemanticMappingTransform(
                new Dictionary<string, string>
                {
                    ["foo"] = "lug",
                    ["bar"] = "baz"
                }
            );
            TransformEmpty = new SemanticMappingTransform();
            TransformNull = new SemanticMappingTransform(null);
        }

        /// <summary>
        /// Tests that when only an overridden property exists, only the property name is changed.
        /// </summary>
        [Test]
        public void TestNameOverride()
        {
            // Transform the vertex.
            FreeformVertex transformed = Transform.Transform(Vertex);

            // Check that the property "foo" has been renamed to "lug" with the value unchanged.
            Assert.AreEqual(3, transformed.Properties.Count);
            Assert.AreEqual("lug", transformed.Properties.Keys[1]);
            Assert.AreEqual(typeof(int), transformed.Properties.Values[1].Type);
            Assert.AreEqual(1, (int)transformed.Properties.Values[1].Value);
        }

        /// <summary>
        /// Tests that when both overridden and overriding properties exist, both the name and value are changed.
        /// </summary>
        [Test]
        public void TestValueOverride()
        {
            // Transform the vertex.
            FreeformVertex transformed = Transform.Transform(Vertex);

            // Check that the property "bar" has been completely replaced (name and value) by the property "baz".
            Assert.AreEqual(3, transformed.Properties.Count);
            Assert.AreEqual("baz", transformed.Properties.Keys[0]);
            Assert.AreEqual(typeof(int), transformed.Properties.Values[0].Type);
            Assert.AreEqual(2, (int)transformed.Properties.Values[0].Value);
        }

        /// <summary>
        /// Tests that when a property is not overridden, it does not change.
        /// </summary>
        [Test]
        public void TestNoOverride()
        {
            // Transform the vertex.
            FreeformVertex transformed = Transform.Transform(Vertex);

            // Check that the property "qux" has not been replaced at all.
            Assert.AreEqual(3, transformed.Properties.Count);
            Assert.AreEqual("qux", transformed.Properties.Keys[2]);
            Assert.AreEqual(typeof(int), transformed.Properties.Values[2].Type);
            Assert.AreEqual(4, (int)transformed.Properties.Values[2].Value);
        }

        /// <summary>
        /// Tests that a semantic mapping with no entries causes no changes.
        /// </summary>
        [Test]
        public void TestEmptyTransform()
        {
            // Transform the vertex with the empty transform.
            FreeformVertex transformed = TransformEmpty.Transform(Vertex);

            // Check that all the properties are the same.
            Assert.AreEqual(Vertex.Properties.Count, transformed.Properties.Count);
            for (int k = 0; k < Vertex.Properties.Count; k++)
            {
                Assert.AreEqual(Vertex.Properties.Keys[k], transformed.Properties.Keys[k]);
                Assert.AreEqual(Vertex.Properties.Values[k].Type, transformed.Properties.Values[k].Type);
                Assert.AreEqual(Vertex.Properties.Values[k].Value, transformed.Properties.Values[k].Value);
            }
        }

        /// <summary>
        /// Tests that a semantic mapping with a null map causes no changes.
        /// </summary>
        [Test]
        public void TestNullTransform()
        {
            // Transform the vertex with the null transform.
            FreeformVertex transformed = TransformNull.Transform(Vertex);

            // Check that all the properties are the same.
            Assert.AreEqual(Vertex.Properties.Count, transformed.Properties.Count);
            for (int k = 0; k < Vertex.Properties.Count; k++)
            {
                Assert.AreEqual(Vertex.Properties.Keys[k], transformed.Properties.Keys[k]);
                Assert.AreEqual(Vertex.Properties.Values[k].Type, transformed.Properties.Values[k].Type);
                Assert.AreEqual(Vertex.Properties.Values[k].Value, transformed.Properties.Values[k].Value);
            }
        }
    }
}