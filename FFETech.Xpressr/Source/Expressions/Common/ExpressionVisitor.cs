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

using System.Collections.Generic;
using System.Text;

namespace FFETech.Xpressr.Expressions
{
    public abstract class ExpressionVisitor
    {
        #region Fields

        List<IExpressionReader> expressionReader = new List<IExpressionReader>();

        #endregion

        #region Properties

        public IEnumerable<IExpressionReader> ExpressionReader
        {
            get { return expressionReader; }
        }

        #endregion

        #region Protected Methods

        protected void RegisterExpressionReader(IExpressionReader expressionReader)
        {
            this.expressionReader.Add(expressionReader);
        }

        protected void Visit(string template)
        {
            if (string.IsNullOrEmpty(template))
                return;

            StringBuilder text = new StringBuilder();

            IExpressionSource source = new StringExpressionSource(template);

            while (source.Index < source.Length)
            {
                bool foundExpression = false;
                foreach (IExpressionReader expressionReader in ExpressionReader)
                {
                    IExpression expression;
                    if (expressionReader.Read(source, out expression))
                    {
                        if (text.Length > 0)
                        {
                            VisitText(text.ToString());
                            text.Clear();
                        }

                        VisitExpression(expression);
                        foundExpression = true;

                        break;
                    }
                }

                if (!foundExpression)
                {
                    text.Append(source.GetChar());
                    source.Read();
                }
            }

            if (text.Length > 0)
                VisitText(text.ToString());
        }

        protected abstract void VisitText(string text);

        protected abstract void VisitExpression(IExpression expression);

        #endregion
    }
}