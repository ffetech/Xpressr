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
    public class SqlSelectExpression : ISqlSelectExpression
    {
        #region Constructors

        public SqlSelectExpression(string name)
        {
            Select = CreateSelectList(name);
            From = CreateFromList(name);
            Where = CreateWhereList(name);
            GroupBy = CreateGroupByList(name);
            Having = CreateHavingList(name);
            OrderBy = CreateOrderByList(name);
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        public IExpressionList<IExpression> Select
        {
            get;
            private set;
        }

        public IExpressionList<IExpression> From
        {
            get;
            private set;
        }

        public IExpressionList<IOperationExpression> Where
        {
            get;
            private set;
        }

        public IExpressionList<IExpression> GroupBy
        {
            get;
            private set;
        }

        public IExpressionList<IOperationExpression> Having
        {
            get;
            private set;
        }

        public IExpressionList<IExpression> OrderBy
        {
            get;
            private set;
        }

        #endregion

        #region Protected Methods

        protected virtual IExpressionList<IExpression> CreateSelectList(string name)
        {
            return new ExpressionList<IExpression>(name);
        }

        protected virtual IExpressionList<IExpression> CreateFromList(string name)
        {
            return new ExpressionList<IExpression>(name);
        }

        protected virtual IExpressionList<IOperationExpression> CreateWhereList(string name)
        {
            return new ExpressionList<IOperationExpression>(name);
        }

        protected virtual IExpressionList<IExpression> CreateGroupByList(string name)
        {
            return new ExpressionList<IExpression>(name);
        }

        protected virtual IExpressionList<IOperationExpression> CreateHavingList(string name)
        {
            return new ExpressionList<IOperationExpression>(name);
        }

        protected virtual IExpressionList<IExpression> CreateOrderByList(string name)
        {
            return new ExpressionList<IExpression>(name);
        }

        #endregion
    }
}