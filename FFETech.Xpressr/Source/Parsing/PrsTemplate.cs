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

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    public class PrsTemplate : PrsRange
    {
        #region Fields

        private Dictionary<string, Type> setup = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        #endregion

        #region Constructors

        public PrsTemplate()
            : base(null)
        {
            SetupElement<PrsContentElement>("Content");
            SetupElement<PrsLoopRange>("Loop");

            SetupElement<PrsFieldElement>("Field");
            SetupElement<PrsRegexElement>("Regex");
        }

        #endregion

        #region Public Methods

        public void SetupElement<T>(string name)
            where T : PrsElement
        {
            setup[name] = typeof(T);
        }

        public void ReadFromTemplate(string template)
        {
            if (Children.Any())
                throw new InvalidOperationException("Document is not empty");

            PrsTemplateReader.Read(this, template);
        }

        public void ReadFromFile(string filePath)
        {
            ReadFromTemplate(File.ReadAllText(filePath));
        }

        public void Parse(string source, IPrsOutput output)
        {
            Parse(new StringExpressionSource(source), output, null);
        }

        #endregion

        #region Internal Methods

        internal override Type GetElementType(string name)
        {
            Type elementType;
            if (!setup.TryGetValue(name, out elementType))
                throw new Exception(string.Format("Unknown element type {0}", name));
            return elementType;
        }

        internal string GetElementName(Type type)
        {
            return setup.Keys.First(key => setup[key] == type);
        }

        #endregion
    }
}