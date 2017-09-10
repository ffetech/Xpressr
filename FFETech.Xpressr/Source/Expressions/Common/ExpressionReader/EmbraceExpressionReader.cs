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

using System;
using System.Text;

namespace FFETech.Xpressr.Expressions
{
    public class EmbraceExpressionReader : IExpressionReader
    {
        #region Constructors

        public EmbraceExpressionReader(string name, string borderLeft, string borderRight, char escapeChar = (char)0)
        {
            Name = name;
            BorderLeft = borderLeft;
            BorderRight = borderRight;
            EscapeChar = escapeChar;
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        public string BorderLeft
        {
            get;
            set;
        }

        public string BorderRight
        {
            get;
            set;
        }

        public char EscapeChar
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        public bool Read(IExpressionSource source, out IExpression expression)
        {
            expression = null;

            if (BorderLeft.Length == 0 || BorderRight.Length == 0)
                throw new InvalidOperationException("Invalid border definition.");

            int i = 0;
            for (; i < BorderLeft.Length && source.Index + i < source.Length; i++)
                if (source.GetChar(i) != BorderLeft[i])
                    return false;

            StringBuilder result = new StringBuilder();

            bool escaped = false;

            int nested = 0;
            int nestedLength = 0;

            for (; source.Index + i < source.Length - (BorderRight.Length - 1); i++)
            {
                if (nestedLength > 0)
                {
                    nestedLength--;
                    if (nestedLength == 0)
                        i += BorderLeft.Length - 1;
                }

                // Verschachtelt
                if (!escaped && BorderLeft != BorderRight)
                {
                    for (int i2 = 0; i2 < BorderLeft.Length; i2++)
                    {
                        char c = source.GetChar(i + i2);
                        char c2 = source.GetChar(i + i2 + 1);

                        if (IsEscapeChar(c, c2))
                            break;

                        if (c != BorderLeft[i2])
                            break;

                        if (i2 == BorderLeft.Length - 1)
                        {
                            nested++;
                            nestedLength = BorderLeft.Length;
                            break;
                        }
                    }
                }

                // Ende
                for (int i2 = 0; i2 < BorderRight.Length; i2++)
                {
                    char c = source.GetChar(i + i2);

                    if (escaped)
                    {
                        escaped = false;
                        result.Append(c);
                        break;
                    }

                    char c2 = source.GetChar(i + i2 + 1);

                    if (IsEscapeChar(c, c2))
                    {
                        escaped = true;
                        break;
                    }

                    result.Append(c);

                    if (c != BorderRight[i2])
                        break;

                    if (i2 == BorderRight.Length - 1)
                    {
                        // Überschneidung
                        if (nestedLength > 0)
                        {
                            if (nested > 0)
                                nested--;
                            nestedLength = 0;
                        }

                        // Verschachtelt
                        if (nested > 0)
                        {
                            nested--;
                            i += BorderRight.Length - 1;
                            break;
                        }
                        else
                        {
                            result.Remove(result.Length - BorderRight.Length, BorderRight.Length);
                            expression = new ValueExpression(Name, result.ToString());
                            source.Read(i + i2 + 1);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private Methods

        private bool IsEscapeChar(char c, char c2)
        {
            if (c2 == (char)0)
                return false;

            if (EscapeChar == (char)0 || c != EscapeChar)
                return false;

            if (EscapeChar.ToString() == BorderLeft)
                return c2 == EscapeChar;

            if (EscapeChar.ToString() == BorderRight)
                return c2 == EscapeChar;

            return true;
        }

        #endregion
    }
}