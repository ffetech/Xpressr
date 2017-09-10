﻿/*

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

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    public class PrsContentElement : PrsElement
    {
        #region Constructors

        public PrsContentElement(PrsRange parent, string content)
            : base(parent)
        {
            Content = content;
            BufferSize = 10000;
        }

        #endregion

        #region Properties

        public string Content
        {
            get;
            protected set;
        }

        public int BufferSize
        {
            get;
            protected set;
        }

        #endregion

        #region Internal Methods

        internal override bool Parse(IExpressionSource source, IPrsOutput output, PrsElement next)
        {
            if (source.GetString(0, Content.Length) == Content)
            {
                output.Debug("Content", Content, source.GetString(0, Content.Length));
                source.Read(Content.Length);
                return true;
            }

            output.Debug("Content", Content, "");
            return false;
        }

        internal override int Search(IExpressionSource source)
        {
            foreach (string searchString in source.BufferString(BufferSize))
            {
                int pos = searchString.IndexOf(Content);
                if (pos >= 0)
                    return pos;
            }

            return -1;
        }

        #endregion
    }
}