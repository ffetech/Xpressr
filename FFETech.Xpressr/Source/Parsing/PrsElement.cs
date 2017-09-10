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

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    public abstract class PrsElement : PrsExpressionTarget
    {
        #region Constructors

        protected PrsElement(PrsRange parent)
        {
            Parent = parent;
        }

        #endregion

        #region Properties

        public PrsRange Parent
        {
            get;
            private set;
        }

        public PrsTemplate Document
        {
            get
            {
                if (this is PrsTemplate)
                    return (PrsTemplate)this;
                return Parent.Document;
            }
        }

        #endregion

        #region Internal Methods

        internal abstract bool Parse(IExpressionSource source, IPrsOutput output, PrsElement next);

        internal virtual int Search(IExpressionSource source)
        {
            return 0;
        }

        #endregion
    }
}