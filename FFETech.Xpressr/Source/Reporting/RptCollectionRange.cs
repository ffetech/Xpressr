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
using System.Linq;
using System.Text;

namespace FFETech.Xpressr.Reporting
{
    public class RptCollectionRange : RptRange
    {
        #region Nested Types

        public enum CollectType
        {
            Record, Field
        }

        private class Record : IRptRecord
        {
            #region Fields

            private Dictionary<string, object> values = new Dictionary<string, object>();

            #endregion

            #region Explicit Members

            bool IRptRecord.TryGetValue(string fieldName, out object value)
            {
                return values.TryGetValue(fieldName, out value);
            }

            #endregion

            #region Public Methods

            public void SetValue(string fieldName, object value)
            {
                values[fieldName] = value;
            }

            #endregion
        }

        private class DataSet : IRptDataSet
        {
            #region Fields

            private List<Record> records = new List<Record>();
            private IEnumerator<Record> recordEnumerator;
            private Dictionary<string, DataSet> children = new Dictionary<string, DataSet>();

            #endregion

            #region Constructors

            public DataSet(string name)
            {
                Name = name;
            }

            #endregion

            #region Explicit Members

            IRptRecord IRptDataSet.CurrentRecord
            {
                get
                {
                    return recordEnumerator?.Current;
                }
            }

            bool IRptDataSet.TryGetDataSet(string datasetName, out IRptDataSet dataSet)
            {
                DataSet result;
                if (children.TryGetValue(datasetName, out result))
                {
                    dataSet = result;
                    return true;
                }

                dataSet = null;
                return false;
            }

            bool IRptDataSet.ReadRecord()
            {
                if (recordEnumerator == null)
                    recordEnumerator = records.GetEnumerator();

                return recordEnumerator.MoveNext();
            }

            #endregion

            #region Properties

            public string Name
            {
                get;
                private set;
            }

            public Record CurrentRecord
            {
                get;
                private set;
            }

            #endregion

            #region Public Methods

            public void NewRecord()
            {
                records.Add(CurrentRecord = new Record());
            }

            public void CollectDataSet(DataSet dataSet)
            {
                foreach (DataSet other in dataSet.children.Values)
                {
                    DataSet child;

                    if (children.TryGetValue(dataSet.Name, out child))
                        child.records.AddRange(other.records);
                    else
                        children.Add(dataSet.Name, other);

                    child.CollectDataSet(other);
                }

                dataSet.children.Clear();
            }

            public DataSet GetOrCreateDataSet(string name)
            {
                DataSet result;

                if (children.TryGetValue(name, out result))
                    return result;

                children.Add(name, result = new DataSet(name));
                return result;
            }

            public bool CascadeDataSet(string datasetName, out IRptDataSet dataSet)
            {
                DataSet result;
                if (children.TryGetValue(datasetName, out result))
                {
                    dataSet = result;
                    return true;
                }

                dataSet = null;
                return false;
            }

            #endregion
        }

        #endregion

        #region Fields

        private DataSet dataSet = new DataSet(string.Empty);
        private DataSet currentDataSet;

        #endregion

        #region Constructors

        public RptCollectionRange(RptRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public CollectType Type
        {
            get;
            protected set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public bool Suppress
        {
            get;
            protected set;
        }

        #endregion

        #region Public Methods

        public bool TryGetDataSet(string datasetName, out IRptDataSet dataSet)
        {
            if (this.dataSet.CascadeDataSet(datasetName, out dataSet))
                return true;

            if (string.IsNullOrEmpty(datasetName))
            {
                dataSet = FindSiblings<RptCollectionRange>().First().dataSet;
                return true;
            }

            return false;
        }

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 2:
                    propertyName = "type";
                    return true;

                case 3:
                    propertyName = "name";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        protected override void DoRender(IRptDataSet dataSet, StringBuilder output)
        {
            StringBuilder tempOutput = new StringBuilder();

            switch (Type)
            {
                case CollectType.Field:

                    base.DoRender(dataSet, tempOutput);

                    RptCollectionRange parentRecord = FindAncestors<RptCollectionRange>().FirstOrDefault(c => c.Type == CollectType.Record);

                    if (parentRecord != null)
                        parentRecord.currentDataSet.CurrentRecord.SetValue(Name, tempOutput);
                    else
                    {
                        DataSet firstCollection = FindSiblings<RptCollectionRange>().First().dataSet;
                        if (firstCollection != null)
                        {
                            if (firstCollection.CurrentRecord == null)
                                firstCollection.NewRecord();
                            firstCollection.CurrentRecord.SetValue(Name, tempOutput);
                        }
                    }

                    break;

                case CollectType.Record:

                    currentDataSet = this.dataSet.GetOrCreateDataSet(Name);
                    currentDataSet.NewRecord();

                    base.DoRender(dataSet, tempOutput);

                    foreach (RptCollectionRange child in FindChildren<RptCollectionRange>().Where(p => p.Type == CollectType.Record))
                        currentDataSet.CollectDataSet(child.dataSet);

                    break;
            }

            if (!Suppress)
                output.Append(tempOutput.ToString());
        }

        #endregion
    }
}