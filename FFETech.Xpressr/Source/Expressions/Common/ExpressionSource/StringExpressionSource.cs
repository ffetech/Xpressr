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
using System.Collections.Generic;

namespace FFETech.Xpressr.Expressions
{
    public class StringExpressionSource : IExpressionSource
    {
        #region Fields

        private string source;
        private int index;
        private Dictionary<object, int> bookmarks;

        #endregion

        #region Constructors

        public StringExpressionSource(string source)
        {
            this.source = source;

            index = 0;
            Length = source.Length;
        }

        #endregion

        #region Properties

        public long Index
        {
            get
            {
                return index;
            }
        }

        public long Length
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public bool Read(int count = 1)
        {
            if (Index >= Length)
                return false;

            index += count;

            return true;
        }

        public char GetChar(int offset = 0)
        {
            if (Index + offset >= Length)
                return (char)0;

            return source[index + offset];
        }

        public string GetString(int startOffset, int length)
        {
            if (Index + startOffset >= Length)
                return string.Empty;

            return source.Substring(index + startOffset, length);
        }

        public IEnumerable<string> BufferString(int bufferSize)
        {
            yield return GetString(0, (int)(Length - Index));
        }

        public void SetBookmark(object owner)
        {
            if (bookmarks == null)
                bookmarks = new Dictionary<object, int>();
            bookmarks[owner] = index;
        }

        public void GotoBookmark(object owner)
        {
            int bookmark;
            if (bookmarks == null || !bookmarks.TryGetValue(owner, out bookmark))
                throw new InvalidOperationException("Invalid bookmark");
            index = bookmark;
        }

        public void ClearBookmark(object owner)
        {
            if (bookmarks == null || !bookmarks.Remove(owner))
                throw new InvalidOperationException("Invalid bookmark");
        }

        #endregion
    }
}