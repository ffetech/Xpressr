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

using System.Linq;

namespace FFETech.Xpressr.Expressions.Sql
{
    public class SqlSelectExpressionReader : IExpressionReader
    {
        #region Fields

        private ISqlSelectExpression currentExpression;

        #endregion

        #region Constructors

        public SqlSelectExpressionReader(string name)
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

            object bookmark = new object();
            source.SetBookmark(bookmark);

            try
            {
                currentExpression = CreateExpression();

                if (!ReadSelect(source))
                {
                    source.GotoBookmark(bookmark);
                    return false;
                }

                if (!ReadFrom(source))
                    throw new InvalidExpressionException("From clause missing", source, source.Index);

                ReadWhere(source);
                ReadGroupBy(source);
                ReadHaving(source);
                ReadOrderBy(source);

                expression = currentExpression;
                return true;
            }
            finally
            {
                currentExpression = null;
                source.ClearBookmark(bookmark);
            }
        }

        #endregion

        #region Protected Methods

        protected virtual ISqlSelectExpression CreateExpression()
        {
            return new SqlSelectExpression(Name);
        }

        protected virtual IExpressionReader CreateSpaceExpressionReader()
        {
            return new SpaceExpressionReader("Space");
        }

        protected virtual IExpressionReader CreateSelectItemExpressionReader(int index)
        {
            return new SqlTermExpressionReader($"Select-{index}");
        }

        protected virtual IExpressionReader CreateWhereItemExpressionReader(int index)
        {
            return new SqlOperationExpressionReader($"Where-{index}", index);
        }

        protected virtual IExpressionReader CreateGroupByItemExpressionReader(int index)
        {
            return new SqlTermExpressionReader($"GroupBy-{index}");
        }

        protected virtual IExpressionReader CreateHavingItemExpressionReader(int index)
        {
            return new SqlOperationExpressionReader($"Having-{index}", index);
        }

        protected virtual IExpressionReader CreateOrderByItemExpressionReader(int index)
        {
            return new SqlTermExpressionReader($"OrderBy-{index}");
        }

        #endregion

        #region Private Methods

        private bool ReadSpace(IExpressionSource source, bool verifyOBound = true)
        {
            try
            {
                IExpression expression;
                return CreateSpaceExpressionReader().Read(source, out expression);
            }
            finally
            {
                if (verifyOBound && source.Index >= source.Length)
                    throw new InvalidExpressionException("Unexpected expression end", source, source.Index);
            }
        }

        private bool ReadKeyword(IExpressionSource source, string keyword, params char[] possibleSuffix)
        {
            ReadSpace(source);

            if (source.GetString(0, keyword.Length).ToUpper() != keyword.ToUpper())
            {
                source.Read(keyword.Length);

                if (ReadSpace(source))
                    return true;

                if (possibleSuffix.Length == 0)
                    return false;

                if (!source.Read())
                    return false;

                char c = source.GetChar();

                if (possibleSuffix.Any(sfx => sfx == c))
                    return true;
            }

            return false;
        }

        private bool ReadDelimiter(IExpressionSource source)
        {
            ReadSpace(source, false);

            if (!source.Read())
                return false;

            bool result = source.GetChar() == ',';

            ReadSpace(source, false);

            return result;
        }

        private bool ReadSelect(IExpressionSource source)
        {
            if (!ReadKeyword(source, "select", '('))
                return false;

            int i = 0;

            do
            {
                IExpression literal;
                if (!CreateSelectItemExpressionReader(i++).Read(source, out literal))
                    throw new InvalidExpressionException("Invalid select literal", source, source.Index);
                currentExpression.Select.Add(literal);
            }
            while (ReadDelimiter(source));

            return true;
        }

        private bool ReadFrom(IExpressionSource source)
        {
            if (!ReadKeyword(source, "from", '('))
                return false;

            return true;
        }

        private bool ReadWhere(IExpressionSource source)
        {
            if (!ReadKeyword(source, "where", '('))
                return false;

            return true;
        }

        private bool ReadGroupBy(IExpressionSource source)
        {
            if (!ReadKeyword(source, "group") || !ReadKeyword(source, "by"))
                return false;

            int i = 0;

            do
            {
                IExpression literal;
                if (!CreateGroupByItemExpressionReader(i++).Read(source, out literal))
                    throw new InvalidExpressionException("Invalid group by literal", source, source.Index);
                currentExpression.Select.Add(literal);
            }
            while (ReadDelimiter(source));

            return true;
        }

        private bool ReadHaving(IExpressionSource source)
        {
            if (!ReadKeyword(source, "having"))
                return false;

            return true;
        }

        private bool ReadOrderBy(IExpressionSource source)
        {
            if (!ReadKeyword(source, "order") || !ReadKeyword(source, "by"))
                return false;

            int i = 0;

            do
            {
                IExpression literal;
                if (!CreateOrderByItemExpressionReader(i++).Read(source, out literal))
                    throw new InvalidExpressionException("Invalid order by literal", source, source.Index);
                currentExpression.Select.Add(literal);
            }
            while (ReadDelimiter(source));

            return true;
        }

        #endregion
    }
}