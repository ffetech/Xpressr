/*

This file is part of Xpressr.

Copyright 2017 FFE-Tech e. U. Inh. Florian Feilmeier
website: www.ffe-tech.com
contact: info@ffe-tech.com

Xpressr is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Xpressr is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with Xpressr.  If not, see <http://www.gnu.org/licenses/>.

*/

namespace FFETech.Xpressr.Expressions.Sql
{
    public class SqlOperationExpressionReader : OperationExpressionReader
    {
        #region Constructors

        public SqlOperationExpressionReader(string name, int index)
            : base(name, index)
        {
        }

        #endregion

        #region Protected Methods

        protected override IExpressionReader CreateBinaryOperatorExpressionReader()
        {
            return new OperatorExpressionReader("BinaryOperator", "and", "or", "=", "<>", ">=", "<=", "in", "is");
        }

        protected override IExpressionReader CreateUnaryOperatorExpressionReader()
        {
            return new OperatorExpressionReader("UnaryOperator", "not");
        }

        protected override IExpressionReader CreateOperandExpressionReader()
        {
            return new SqlTermExpressionReader("Operand");
        }

        protected override IExpressionReader CreateChildExpressionReader(int index)
        {
            return new SqlOperationExpressionReader($"{Name}-Child-{index}", index);
        }

        #endregion
    }
}