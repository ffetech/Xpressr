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
    public interface IExpressionSource
    {
        #region Properties

        long Index
        {
            get;
        }

        long Length
        {
            get;
        }

        #endregion

        #region Methods

        bool Read(int count = 1);

        char GetChar(int offset = 0);

        string GetString(int startOffset, int length);

        IEnumerable<string> BufferString(int bufferSize);

        void SetBookmark(object owner);

        void GotoBookmark(object owner);

        void ClearBookmark(object owner);

        #endregion
    }
}