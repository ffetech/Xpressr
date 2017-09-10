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

namespace FFETech.Xpressr.Expressions
{
    public class TokenExpressionReader : IExpressionReader
    {
        #region Constructors

        public TokenExpressionReader(string name, string delimiter, IEnumerable<IExpressionReader> innerExpressions = null)
        {
            Name = name;
            Delimiter = delimiter;
            InnerExpressions = innerExpressions;
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        public string Delimiter
        {
            get;
            set;
        }

        public IEnumerable<IExpressionReader> InnerExpressions
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        public bool Read(IExpressionSource source, out IExpression expression)
        {
            expression = null;

            if (source.Index >= source.Length)
                return false;

            object bookmark = new object();
            source.SetBookmark(bookmark);

            try
            {
                while (source.Index < source.Length - Delimiter.Length)
                {
                    bool innerExpressionFound = false;

                    if (InnerExpressions != null)
                    {
                        IExpression innerExpression;
                        foreach (IExpressionReader innerExpressionReader in InnerExpressions)
                            if (innerExpressionReader.Read(source, out innerExpression))
                            {
                                innerExpressionFound = true; // -1
                                break;
                            }
                    }

                    if (!innerExpressionFound && source.GetString(0, Delimiter.Length) == Delimiter)
                    {
                        long index = source.Index;
                        source.GotoBookmark(bookmark);
                        int length = (int)(index - source.Index);

                        expression = new ValueExpression(Name, source.GetString(0, length));
                        source.Read(length + Delimiter.Length);
                        return true;
                    }

                    if (!innerExpressionFound)
                        source.Read();
                }

                source.GotoBookmark(bookmark);
                int finalLength = (int)(source.Length - source.Index);

                expression = new ValueExpression(Name, source.GetString(0, finalLength));
                source.Read(finalLength);
                return true;
            }
            finally
            {
                source.ClearBookmark(bookmark);
            }
        }

        public IEnumerable<IExpression> ReadAll(string template)
        {
            IExpression expression;
            IExpressionSource source = new StringExpressionSource(template);
            while (((IExpressionReader)this).Read(source, out expression))
                yield return expression;
        }

        #endregion
    }
}