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

using System.Text;

namespace FFETech.Xpressr.Expressions
{
    public class OperandExpressionReader : IExpressionReader
    {
        #region Constructors

        public OperandExpressionReader(string name)
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

            StringBuilder value = new StringBuilder();
            bool finalized = false;

            int nestedIndexer = 0;

            while (!finalized && source.Index < source.Length)
            {
                char c = source.GetChar();

                if (!(char.IsLetterOrDigit(c)))
                {
                    if (value.Length == 0)
                        finalized = true;
                    else
                    {
                        switch (c)
                        {
                            // ToDo: Korrekte Syntax für Indexer und Qualifizierung
                            case '_':
                            case '.':
                                break;

                            case '[':
                                nestedIndexer++;
                                break;

                            case ']':
                                if (nestedIndexer > 0)
                                    nestedIndexer--;
                                else
                                    finalized = true;
                                break;

                            default:
                                finalized = true;
                                break;
                        }
                    }
                }

                if (!finalized)
                {
                    value.Append(c);
                    source.Read();
                }
            }

            if (value.Length == 0)
                return false;

            // ToDo: Komplexere Expression mit Qualifizierung und Indexern abgeleitet von IValueExpression
            expression = new ValueExpression(Name, value.ToString());
            return true;
        }

        #endregion
    }
}