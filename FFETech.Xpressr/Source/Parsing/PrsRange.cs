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
using System.Linq;

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    public abstract class PrsRange : PrsElement
    {
        #region Fields

        private List<PrsElement> children = new List<PrsElement>();

        #endregion

        #region Constructors

        public PrsRange(PrsRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public IEnumerable<PrsElement> Children
        {
            get
            {
                return children;
            }
        }

        #endregion

        #region Internal Methods

        internal virtual Type GetElementType(string name)
        {
            return Parent.GetElementType(name);
        }

        internal PrsElement CreateChild(Type elementType)
        {
            return (PrsElement)Activator.CreateInstance(elementType, this);
        }

        internal void AddChild(PrsElement child)
        {
            if (child.Parent != this)
                throw new InvalidOperationException("Parent invalid");
            children.Add(child);
        }

        internal override bool Parse(IExpressionSource source, IPrsOutput output, PrsElement next)
        {
            bool result = false;

            PrsElement[] elements = Children.ToArray();

            output.Debug("Range begin", elements.Length.ToString());

            for (int i = 0; i < elements.Length; i++)
            {
                object bookmark = new object();
                source.SetBookmark(bookmark);

                try
                {
                    PrsElement element = elements[i];
                    PrsElement nextElement = i + 1 < elements.Length ? elements[i + 1] : null;

                    if (!element.Parse(source, output, nextElement))
                    {
                        source.GotoBookmark(bookmark);
                        break;
                    }

                    result = true;
                }
                finally
                {
                    source.ClearBookmark(bookmark);
                }
            }

            output.Debug("Range end");

            return result;
        }

        #endregion
    }
}