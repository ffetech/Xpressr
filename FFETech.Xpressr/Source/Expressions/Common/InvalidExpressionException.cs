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
    public sealed class InvalidExpressionException : Exception
    {
        #region Constructors

        public InvalidExpressionException(string message, IExpressionSource expressionSource, long startIndex)
            : base(message)
        {
            ExpressionSource = expressionSource;
        }

        public InvalidExpressionException(string message, IExpressionSource source, long startIndex, Exception innerException)
            : base(message, innerException)
        {
            ExpressionSource = source;
        }

        #endregion

        #region Properties

        public IExpressionSource ExpressionSource
        {
            get;
            private set;
        }

        public long StartIndex
        {
            get;
            private set;
        }

        #endregion
    }
}