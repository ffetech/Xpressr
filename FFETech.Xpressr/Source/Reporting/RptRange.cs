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
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public abstract class RptRange : RptElement
    {
        #region Fields

        private List<RptElement> children = new List<RptElement>();
        private int currentRecordCount;
        private int currentCount;

        #endregion

        #region Constructors

        public RptRange(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public IEnumerable<RptElement> Children
        {
            get
            {
                return children;
            }
        }

        public int CurrentRecordCount
        {
            get
            {
                return currentRecordCount;
            }
        }

        public int CurrentCount
        {
            get
            {
                return currentCount;
            }
        }

        public int Limit
        {
            get;
            protected set;
        }

        #endregion

        #region Public Methods

        public IEnumerable<T> FindChildren<T>()
            where T : RptElement
        {
            return Children.OfType<T>();
        }

        public IEnumerable<T> FindDescendants<T>()
            where T : RptElement
        {
            foreach (T child in FindChildren<T>())
            {
                yield return child;

                RptRange rangeChild = child as RptRange;

                if (rangeChild != null)
                    foreach (T descendant in rangeChild.FindDescendants<T>())
                        yield return descendant;
            }
        }

        public void ResetLimit()
        {
            currentCount = 0;
        }

        public bool LimitReached()
        {
            return Limit > 0 && currentCount >= Limit;
        }

        public virtual bool Join(IRptDataSet dataSet)
        {
            currentRecordCount++;

            if (!(this is RptDataSetRange) && !(Parent?.Join(dataSet) ?? false))
                return false;

            foreach (RptAggregateElement aggregate in Children.OfType<RptAggregateElement>())
                aggregate.Join(dataSet);

            return true;
        }

        #endregion

        #region Internal Methods

        internal virtual Type GetElementType(string name)
        {
            return Parent.GetElementType(name);
        }

        internal RptElement CreateChild(Type elementType)
        {
            return (RptElement)Activator.CreateInstance(elementType, this);
        }

        internal void AddChild(RptElement child)
        {
            if (child.Parent != this)
                throw new InvalidOperationException("Parent invalid");
            children.Add(child);
        }

        #endregion

        #region Protected Methods

        protected void RenderItems(IRptDataSet dataSet, StringBuilder output, IEnumerable<RptElement> items)
        {
            currentRecordCount = 0;

            foreach (RptAggregateElement aggregate in items.OfType<RptAggregateElement>())
                aggregate.Reset();

            Dictionary<RptElement, StringBuilder> childOutputs = new Dictionary<RptElement, StringBuilder>();

            foreach (RptElement child in items.Where(c => !(c is RptGroupRange) && !(c is RptRecordRange) && !(c is RptAggregateElement)))
            {
                childOutputs.Add(child, new StringBuilder());
                child.Render(dataSet, childOutputs[child]);
            }

            foreach (RptElement child in items.Where(c => (c is RptGroupRange) || (c is RptRecordRange) || (c is RptAggregateElement)))
            {
                childOutputs.Add(child, new StringBuilder());
                child.Render(dataSet, childOutputs[child]);
            }

            foreach (RptElement child in items)
            {
                output.Append(childOutputs[child].ToString());
            }
        }

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            currentCount++;

            if (!LimitReached())
                RenderItems(dataSet, output, Children);
            else
            {
                while (dataSet.CurrentRecord != null && Join(dataSet))
                    dataSet.ReadRecord();
            }
        }

        #endregion
    }
}