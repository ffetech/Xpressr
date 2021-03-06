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

using System;
using System.Collections.Generic;
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public abstract class RptElement : RptExpressionTarget
    {
        #region Constructors

        protected RptElement(RptRange parent)
        {
            Parent = parent;
        }

        #endregion

        #region Properties

        public RptRange Parent
        {
            get;
            private set;
        }

        public RptDocument Document
        {
            get
            {
                if (this is RptDocument)
                    return (RptDocument)this;
                return Parent.Document;
            }
        }

        #endregion

        #region Public Methods

        public IEnumerable<T> FindAncestors<T>()
            where T : RptRange
        {
            if (Parent != null)
            {
                if (Parent is T)
                    yield return (T)Parent;

                foreach (T ancestor in Parent.FindAncestors<T>())
                    yield return ancestor;
            }
        }

        public IEnumerable<T> FindSiblings<T>()
            where T : RptElement
        {
            if (Parent != null)
                foreach (T siebling in Parent.FindChildren<T>())
                    yield return siebling;
        }

        public void Render(IRptDataSet dataSet, StringBuilder output)
        {
            ExecuteBindings(dataSet);
            try
            {
                DoRender(dataSet, output);
            }
            catch (RptDataException ex)
            {
                output.Append($"#{ex.Message}#");
            }
        }

        #endregion

        #region Protected Methods

        protected abstract void DoRender(IRptDataSet dataSet, StringBuilder output);

        #endregion
    }
}