using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcTree.Core;

namespace Core.Test
{
    [TestClass]
    public class TestDdlCreation
    {
        private readonly DbField _numeric152 = new DbField
        {
            BaseType = DbFirebirdBaseFieldType.Long,
            SubType = DbFirebirdBaseFieldSubType.Numeric,
            Precision = 15,
            Scale = 2
        };

        private readonly DbField _decimal104 = new DbField
        {
            BaseType = DbFirebirdBaseFieldType.Long,
            SubType = DbFirebirdBaseFieldSubType.Decimal,
            Precision = 10,
            Scale = 4
        };

        private readonly DbField _varchar100 = new DbField
        {
            BaseType = DbFirebirdBaseFieldType.Varying,
            CharacterLength = 100,
            CharacterSet = new CharacterSet
            {
                Name = "EUCJ_0208"
            }
        };

        private readonly DbField _char100 = new DbField
        {
            BaseType = DbFirebirdBaseFieldType.Text,
            CharacterLength = 100,
            CharacterSet = new CharacterSet
            {
                Name = "EUCJ_0208"
            }
        };

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
                Assert.AreEqual(field.GetDdl(), field.BaseType.ToString().ToLower());
            }
        }

        [TestMethod]
        public void TestSimpleNumericTypesDdl()
        {
            var fieldSmallint = new DbField {BaseType = DbFirebirdBaseFieldType.Short};
            Assert.AreEqual(fieldSmallint.GetDdl(), "smallint");

            var fieldInteger = new DbField {BaseType = DbFirebirdBaseFieldType.Long};
            Assert.AreEqual(fieldInteger.GetDdl(), "integer");

            var fieldBigint = new DbField {BaseType = DbFirebirdBaseFieldType.Int64};
            Assert.AreEqual(fieldBigint.GetDdl(), "bigint");

            fieldBigint = new DbField {BaseType = DbFirebirdBaseFieldType.Quad};
            Assert.AreEqual(fieldBigint.GetDdl(), "bigint");
        }

        [TestMethod]
        public void TestComplexNumericTypesDdl()
        {
            Assert.AreEqual(_numeric152.GetDdl(), "numeric(15,2)");
            Assert.AreEqual(_decimal104.GetDdl(), "decimal(10,4)");
        }

        [TestMethod]
        public void TestSimpleTextDdl()
        {
            _varchar100.CharacterSet = null;
            Assert.AreEqual(_varchar100.GetDdl(), "varchar(100)");

            _char100.CharacterSet = null;
            Assert.AreEqual(_char100.GetDdl(), "char(100)");
        }

        [TestMethod]
        public void TestTextCharacterSetDdl()
        {
            Assert.AreEqual(_varchar100.GetDdl(), "varchar(100) character set EUCJ_0208");
            Assert.AreEqual(_char100.GetDdl(), "char(100) character set EUCJ_0208");            
        }

        [TestMethod]
        public void TestProcedureTypeOfParamMechanismDdl()
        {
            var paramTypeOf = new DbProcedureParameter
            {
                Name = "par2",
                Field = _numeric152,
                IsAllowNull = true,
                ParameterMechanism = ParameterMechanism.TypeOf,
                FieldName = "supersum",
                RelationName = "test"
            };
            Assert.AreEqual(paramTypeOf.GetDdl(), "PAR2 type of column TEST.SUPERSUM");
        }

        [TestMethod]
        public void TestSimpleProcedureNormalParameterDdl()
        {
            var paramNormal = new DbProcedureParameter
            {
                Name = "par1",
                Field = _numeric152,
                IsAllowNull = true,
                ParameterMechanism = ParameterMechanism.Normal
            };
            Assert.AreEqual(paramNormal.GetDdl(), "PAR1 numeric(15,2)");            
        }

        [TestMethod]
        public void TestNotNullProcedureNormalParameterDdl()
        {
            var paramNormal = new DbProcedureParameter
            {
                Name = "par1",
                Field = _numeric152,
                IsAllowNull = false,
                ParameterMechanism = ParameterMechanism.Normal
            };
            Assert.AreEqual(paramNormal.GetDdl(), "PAR1 numeric(15,2) not null");  
        }

        [TestMethod]
        public void TestCharacterSetProcedureNormalParameterDdl()
        {
            var paramNormal = new DbProcedureParameter
            {
                Name = "par1",
                Field = _varchar100,
                IsAllowNull = true,
                ParameterMechanism = ParameterMechanism.Normal
            };
            Assert.AreEqual(paramNormal.GetDdl(), "PAR1 varchar(100) character set EUCJ_0208");
        }

        [TestMethod]
        public void TestCharacterSetCollationProcedureNormalParameterDdl()
        {
            _varchar100.Collation = new Collation
            {
                Name = "EUCJ_0208"
            };
            var paramNormal = new DbProcedureParameter
            {
                Name = "par1",
                Field = _varchar100,
                IsAllowNull = false,
                ParameterMechanism = ParameterMechanism.Normal
            };
            Assert.AreEqual(paramNormal.GetDdl(), "PAR1 varchar(100) character set EUCJ_0208 not null collation EUCJ_0208");
        }

        [TestMethod]
        public void TestCharacterSetCollation2ProcedureNormalParameterDdl()
        {
            var paramNormal = new DbProcedureParameter
            {
                Name = "par1",
                Field = _varchar100,
                Collation = new Collation
                {
                    Name = "EUCJ_0208"
                },
                IsAllowNull = false,
                ParameterMechanism = ParameterMechanism.Normal
            };
            Assert.AreEqual(paramNormal.GetDdl(), "PAR1 varchar(100) character set EUCJ_0208 not null collation EUCJ_0208");           
        }

        [TestMethod]
        public void TestProcedureNormalParameterWithDefaultDdl()
        {
            var paramNormal = ParamNormal;
            Assert.AreEqual(paramNormal.GetDdl(), "PAR1 varchar(100) character set EUCJ_0208 not null collation EUCJ_0208 = null");
        }

        private DbProcedureParameter ParamNormal
        {
            get
            {
                return new DbProcedureParameter
                {
                    Name = "par1",
                    Field = _varchar100,
                    Collation = new Collation
                    {
                        Name = "EUCJ_0208"
                    },
                    IsAllowNull = false,
                    ParameterMechanism = ParameterMechanism.Normal,
                    DefaultSource = "= null"
                };
            }
        }

        [TestMethod]
        public void TestSimpleProcedureDdl()
        {
            var paramList = new List<DbProcedureParameter>();
            var param1 = new DbProcedureParameter
            {
                Name = "QUARTER",
                ParameterType = ParameterType.Input,
                ParameterMechanism = ParameterMechanism.TypeOf,
                FieldName = "K_NUM",
                RelationName = "DOC_ORDER"
            };
            var param2 = new DbProcedureParameter
            {
                Name = "ZAYAVKA_YEAR",
                ParameterType = ParameterType.Input,
                ParameterMechanism = ParameterMechanism.TypeOf,
                FieldName = "K_YEAR",
                RelationName = "DOC_ORDER"
            };
            var param3 = new DbProcedureParameter
            {
                Name = "ID_ART",
                ParameterType = ParameterType.Input,
                ParameterMechanism = ParameterMechanism.TypeOf,
                FieldName = "ID_ART",
                RelationName = "DOC_BY_ZAYAVKA_H"
            };
            var param4 = new DbProcedureParameter
            {
                Name = "ID_CFO",
                ParameterType = ParameterType.Input,
                ParameterMechanism = ParameterMechanism.TypeOf,
                FieldName = "ID_CFO",
                RelationName = "DOC_BY_ZAYAVKA_H"
            };
            var param5 = new DbProcedureParameter
            {
                Name = "USER_ID",
                ParameterType = ParameterType.Input,
                ParameterMechanism = ParameterMechanism.TypeOf,
                FieldName = "ID",
                RelationName = "USER_BUDGET"
            };
            var param6 = new DbProcedureParameter
            {
                Name = "CORRECT_ID",
                ParameterType = ParameterType.Output,
                ParameterMechanism = ParameterMechanism.TypeOf,
                FieldName = "ID",
                RelationName = "DOC_BY_CORRECT_H"
            };
            paramList.AddRange(new[] {param1, param2, param3, param4, param5, param6});
            var proc = new DbProcedure
            {
                Name = "CREATE_UPP_YEAR_CORRECT",
                Type = DbObjectType.Procedure,
                Parameters = paramList,
                Source =
                    "begin\r\ninsert into doc_by_correct_h (tip, vd, c_year, c_text, id_art, id_cfo, is_checking, is_checked, is_signed, is_permitted, is_del)\r\nvalues (3, :quarter, :zayavka_year, '', :id_art, :id_cfo, 0, 0, 0, 0, 0)\r\nreturning id into :correct_id;\r\n\r\ninsert into doc_action (id_doc, id_act, vid, act_user_id)\r\nvalues (:correct_id, 1, 8, :id_user);\r\n\r\nsuspend;\r\nend"
            };
            Assert.AreEqual(proc.GetDdl(), "");
        }
    }
}
