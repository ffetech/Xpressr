using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FFETech.Xpressr.Expressions.Tests
{
    [TestClass]
    public class ExpressionReaderTests
    {
        #region Public Methods

        [TestMethod]
        public void EmbraceExpressionReaderTest1()
        {
            IExpressionReader reader = new EmbraceExpressionReader("test", "~#", "#~", '\\');
            IValueExpression valueExpression = null;

            valueExpression = Read<IValueExpression>(reader, "~#  Das ist ein Test 5 + 9 = 14 ~#~");
            Assert.AreEqual(valueExpression.Value, "  Das ist ein Test 5 + 9 = 14 ~");

            valueExpression = Read<IValueExpression>(reader, "~# Das ist ein ~#  Test #~\\~#~");
            Assert.AreEqual(valueExpression.Value, " Das ist ein ~#  Test #~~");

            valueExpression = Read<IValueExpression>(reader, "~#~Das ist ein ~#Test~#test#~#~\\~#~ + - ausserhalb");
            Assert.AreEqual(valueExpression.Value, "~Das ist ein ~#Test~#test#~#~~");

            Read<IValueExpression>(reader, "~# Das ist ein Test # ~# xyz", false);
            Read<IValueExpression>(reader, " ~# Das ist ein Test #~", false);
        }

        [TestMethod]
        public void EmbraceExpressionReaderTest2()
        {
            IExpressionReader reader = new EmbraceExpressionReader("test", "\"", "\"", '/');
            IValueExpression valueExpression = null;

            valueExpression = Read<IValueExpression>(reader, "\"  Das ist ein Test 5 + 9 = 14 ~\"");
            Assert.AreEqual(valueExpression.Value, "  Das ist ein Test 5 + 9 = 14 ~");

            valueExpression = Read<IValueExpression>(reader, "\" Das ist ein ~#  Test #~\\~\"");
            Assert.AreEqual(valueExpression.Value, " Das ist ein ~#  Test #~\\~");

            valueExpression = Read<IValueExpression>(reader, "\"~Das ist ein \"Test\"test\"\"/~\" + - ausserhalb");
            Assert.AreEqual(valueExpression.Value, "~Das ist ein ");

            Read<IValueExpression>(reader, " Das ist ein Test /\" xyz", false);
        }

        #endregion

        #region Protected Methods

        protected T Read<T>(IExpressionReader reader, IExpressionSource source, bool expectedResult = true)
            where T : IExpression
        {
            IExpression exp;
            bool result = reader.Read(source, out exp);

            Assert.AreEqual(result, expectedResult);

            if (expectedResult)
            {
                Assert.IsInstanceOfType(exp, typeof(T));
                return (T)exp;
            }

            Assert.IsNull(exp);
            return default(T);
        }

        protected T Read<T>(IExpressionReader reader, string source, bool expectedResult = true)
            where T : IExpression
        {
            return Read<T>(reader, new StringExpressionSource(source), expectedResult);
        }

        #endregion
    }
}