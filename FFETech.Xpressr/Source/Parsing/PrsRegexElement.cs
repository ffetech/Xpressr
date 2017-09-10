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
using System.Text.RegularExpressions;

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    public class PrsRegexElement : PrsElement
    {
        #region Constructors

        public PrsRegexElement(PrsRange parent)
            : base(parent)
        {
        }

        #endregion

        #region Properties

        public string Pattern
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

        #region Public Methods

        public bool ExecuteMatch(string searchString, out Match match)
        {
            Regex regex = new Regex(Pattern, RegexOptions.Singleline);
            Func<Match> executeMatch = () => regex.Match(searchString);

            var wh = executeMatch.BeginInvoke(null, null);

            if (wh.AsyncWaitHandle.WaitOne(500))
            {
                match = executeMatch.EndInvoke(wh);
                return match.Success;
            }

            match = null;
            return false;
        }

        #endregion

        #region Internal Methods

        internal override bool Parse(IExpressionSource source, IPrsOutput output, PrsElement next)
        {
            int index, length;

            if (Execute(source, out index, out length, next) && index == 0)
            {
                output.Debug("Regex", Pattern, source.GetString(0, length));
                source.Read(length);
                return true;
            }

            output.Debug("Regex", Pattern, "");
            return false;
        }

        internal override int Search(IExpressionSource source)
        {
            int index, length;

            if (Execute(source, out index, out length, null))
                return index;

            return -1;
        }

        #endregion

        #region Protected Methods

        protected override bool GetExpressionDefaultProperty(int index, out string propertyName)
        {
            switch (index)
            {
                case 1:
                    propertyName = "pattern";
                    return true;
            }

            return base.GetExpressionDefaultProperty(index, out propertyName);
        }

        #endregion

        #region Private Methods

        private bool Execute(IExpressionSource source, out int index, out int length, PrsElement next)
        {
            foreach (string searchString in source.BufferString(BufferSize))
            {
                Match match;
                if (ExecuteMatch(searchString, out match))
                {
                    index = match.Index;
                    length = match.Length;
                    return true;
                }
            }

            index = -1;
            length = 0;
            return false;
        }

        #endregion
    }
}