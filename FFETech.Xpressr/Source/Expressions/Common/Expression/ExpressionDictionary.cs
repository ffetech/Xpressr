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
using System.Collections;
using System.Collections.Generic;

namespace FFETech.Xpressr.Expressions
{
    public class ExpressionDictionary<T> : IExpressionDictionary<T>
        where T : IExpression
    {
        #region Fields

        private Dictionary<string, T> dictionary;

        #endregion

        #region Constructors

        public ExpressionDictionary(string name)
        {
            Name = name;
            dictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Explicit Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
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

        public virtual void Add(string key, T value)
        {
            dictionary.Add(key, value);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        #endregion
    }
}