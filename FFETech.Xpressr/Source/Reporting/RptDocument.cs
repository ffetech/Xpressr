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
using System.IO;
using System.Linq;
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptDocument : RptRange
    {
        #region Nested Types

        private class CollectionDataSetImpl : IRptDataSet
        {
            #region Fields

            private RptDocument document;
            private IRptDataSet currentDataSet;

            #endregion

            #region Constructors

            public CollectionDataSetImpl(RptDocument document)
            {
                this.document = document;
            }

            #endregion

            #region Explicit Members

            IRptRecord IRptDataSet.CurrentRecord
            {
                get
                {
                    return currentDataSet?.CurrentRecord;
                }
            }

            bool IRptDataSet.TryGetDataSet(string datasetName, out IRptDataSet dataSet)
            {
                if (string.IsNullOrEmpty(datasetName))
                {
                    dataSet = GetCurrentDataSet();
                    return true;
                }

                foreach (RptCollectionRange collection in document.FindChildren<RptCollectionRange>().Where(c => c.Type == RptCollectionRange.CollectType.Record))
                {
                    if (collection.TryGetDataSet(datasetName, out dataSet))
                        return true;
                }

                dataSet = null;
                return false;
            }

            bool IRptDataSet.ReadRecord()
            {
                return GetCurrentDataSet()?.ReadRecord() ?? false;
            }

            #endregion

            #region Private Methods

            private IRptDataSet GetCurrentDataSet()
            {
                if (currentDataSet != null)
                    return currentDataSet;

                document.FindChildren<RptCollectionRange>().FirstOrDefault()?.TryGetDataSet(string.Empty, out currentDataSet);

                return currentDataSet;
            }

            #endregion
        }

        #endregion

        #region Fields

        private Dictionary<string, Type> setup = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private IRptDataSet collectionDataSet;

        #endregion

        #region Constructors

        public RptDocument()
            : base(null)
        {
            SetupElement<RptContentElement>("Content");
            SetupElement<RptDataSetRange>("DataSet");
            SetupElement<RptGroupRange>("Group");
            SetupElement<RptRecordRange>("Record");
            SetupElement<RptConditionRange>("Condition");
            SetupElement<RptPrototypeRange>("Prototype");
            SetupElement<RptCollectionRange>("Collection");

            SetupElement<RptFieldElement>("Field");
            SetupElement<RptExpressionElement>("Expression");
            SetupElement<RptLookupElement>("Lookup");

            SetupElement<RptCountAggregateElement>("Count");
            SetupElement<RptSumAggregateElement>("Sum");
            SetupElement<RptAverageAggregateElement>("Avarage");

            SetupElement<RptChartElement>("Chart");
        }

        #endregion

        #region Properties

        public IRptDataSet CollectionDataSet
        {
            get
            {
                return collectionDataSet ?? (collectionDataSet = new CollectionDataSetImpl(this));
            }
        }

        #endregion

        #region Public Methods

        public void SetupElement<T>(string name)
            where T : RptElement
        {
            setup[name] = typeof(T);
        }

        public void ReadFromTemplate(string template)
        {
            if (Children.Any())
                throw new InvalidOperationException("Document is not empty");

            RptTemplateReader.Read(this, template);
        }

        public void ReadFromFile(string filePath)
        {
            ReadFromTemplate(File.ReadAllText(filePath));
        }

        #endregion

        #region Internal Methods

        internal override Type GetElementType(string name)
        {
            Type elementType;
            if (!setup.TryGetValue(name, out elementType))
                throw new RptTemplateException(string.Format("Unknown element type {0}", name));
            return elementType;
        }

        internal string GetElementName(Type type)
        {
            return setup.Keys.First(key => setup[key] == type);
        }

        #endregion

        #region Protected Methods

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            if (dataSet.CurrentRecord == null)
                dataSet.ReadRecord();

            base.DoRender(dataSet, output);
        }

        #endregion
    }
}