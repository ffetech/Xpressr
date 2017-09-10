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

namespace FFETech.Xpressr.Expressions
{
    public class OperatorExpressionReader : IExpressionReader
    {
        #region Constructors

        public OperatorExpressionReader(string name, params string[] operators)
        {
            Name = name;
            Operators = operators;
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        public string[] Operators
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
                return true;
            }
            finally
            {
                source.ClearBookmark(bookmark);
            }
        }

        #endregion
    }
}