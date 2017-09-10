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

namespace FFETech.Xpressr.Expressions
{
    public class ParamExpressionReader : IExpressionReader
    {
        #region Constructors

        public ParamExpressionReader(string name, string prefix)
        {
            Name = name;
            Prefix = prefix;
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        public string Prefix
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        public bool Read(IExpressionSource source, out IExpression expression)
        {
            expression = null;

            if (Prefix.Length == 0)
                throw new InvalidOperationException("Invalid prefix definition.");

            int i = 0;
            for (; i < Prefix.Length && source.Index + i < source.Length; i++)
                if (source.GetChar(i) != Prefix[i])
                    return false;

            for (; source.Index + i < source.Length; i++)
            {
                char c = source.GetChar(i);
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    break;
            }

            int iLength = (int)((source.Index + i) - source.Index - 1);
            expression = new ValueExpression(Name, source.GetString(1, iLength));
            source.Read(iLength + 1);
            return true;
        }

        #endregion
    }
}