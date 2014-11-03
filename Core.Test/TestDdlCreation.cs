using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcTree.Core;

namespace Core.Test
{
    [TestClass]
    public class TestDdlCreation
    {
        [TestMethod]
        public void TestSimpleTypesDdl()
        {
            var fieldArr = new[]
            {
                new DbField {BaseType = DbFirebirdBaseFieldType.Blob},
                new DbField {BaseType = DbFirebirdBaseFieldType.Date},
                new DbField {BaseType = DbFirebirdBaseFieldType.Double},
                new DbField {BaseType = DbFirebirdBaseFieldType.Float},
                new DbField {BaseType = DbFirebirdBaseFieldType.Time},
                new DbField {BaseType = DbFirebirdBaseFieldType.Timestamp}
            };
            foreach (var field in fieldArr)
            {
                Assert.AreEqual(field.GetDdl(), field.BaseType.ToString().ToUpper());
            }
        }

        [TestMethod]
        public void TestSimpleNumericTypesDdl()
        {
            var fieldSmallint = new DbField {BaseType = DbFirebirdBaseFieldType.Short};
            Assert.AreEqual(fieldSmallint.GetDdl(), "SMALLINT");

            var fieldInteger = new DbField {BaseType = DbFirebirdBaseFieldType.Long};
            Assert.AreEqual(fieldInteger.GetDdl(), "INTEGER");

            var fieldBigint = new DbField {BaseType = DbFirebirdBaseFieldType.Int64};
            Assert.AreEqual(fieldBigint.GetDdl(), "BIGINT");

            fieldBigint = new DbField {BaseType = DbFirebirdBaseFieldType.Quad};
            Assert.AreEqual(fieldBigint.GetDdl(), "BIGINT");
        }

        [TestMethod]
        public void TestComplexNumericTypesDdl()
        {
            var numeric152 = new DbField
            {
                BaseType = DbFirebirdBaseFieldType.Long,
                SubType = DbFirebirdBaseFieldSubType.Numeric,
                Precision = 15,
                Scale = 2
            };
            Assert.AreEqual(numeric152.GetDdl(), "NUMERIC(15,2)");

            var decimal104 = new DbField
            {
                BaseType = DbFirebirdBaseFieldType.Long,
                SubType = DbFirebirdBaseFieldSubType.Decimal,
                Precision = 10,
                Scale = 4
            };
            Assert.AreEqual(decimal104.GetDdl(), "DECIMAL(10,4)");
        }
    }
}
