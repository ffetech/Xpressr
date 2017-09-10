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
    public class SqlTermExpressionReader : IExpressionReader
    {
        #region Constructors

        public SqlTermExpressionReader(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public bool Read(IExpressionSource source, out IExpression expression)
        {
            expression = null;
            return false;
        }

        #endregion

        #region Protected Methods

        protected virtual IExpressionReader CreateOperandExpressionReader(string name)
        {
            return new OperandExpressionReader(name);
        }

        protected virtual IExpressionReader CreateStringLiteralExpressionReader(string name, char embrace)
        {
            return new EmbraceExpressionReader(name, embrace.ToString(), embrace.ToString(), '\\');
        }

        protected virtual IExpressionReader CreateFunctionExpressionReader(string name)
        {
            return new FunctionExpressionReader(name);
        }

        protected virtual IExpressionReader CreateSelectExpressionReader(string name)
        {
            return new SqlSelectExpressionReader(name);
        }

        #endregion
    }
}