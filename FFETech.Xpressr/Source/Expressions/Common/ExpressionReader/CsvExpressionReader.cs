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
using System.Linq;
using System.Text;

namespace FFETech.Xpressr.Expressions
{
    public class CsvExpressionReader : IExpressionReader
    {
        #region Fields

        private SpaceExpressionReader spaceExpressionReader;
        private EmbraceExpressionReader quotedExpressionReader;

        #endregion

        #region Constructors

        public CsvExpressionReader(string name, string[] delimiters)
        {
            Name = name;
            Delimiters = delimiters;

            quotedExpressionReader = new EmbraceExpressionReader(name, "\"", "\"", '\\');

            spaceExpressionReader = new SpaceExpressionReader(name);
            spaceExpressionReader.SpaceChars = spaceExpressionReader.SpaceChars.Except(new[] { '\r', '\n' }).ToArray();
        }

        public CsvExpressionReader(string name, string delimiter)
            : this(name, new[] { delimiter })
        {
        }

        #endregion

        #region Properties

        public string Name
        {
            get;
            private set;
        }

        public string[] Delimiters
        {
            get;
            set;
        }

        public char QuoteChar
        {
            get
            {
                return quotedExpressionReader.BorderLeft[0];
            }
            set
            {
                quotedExpressionReader.BorderLeft = new string(new[] { value });
                quotedExpressionReader.BorderRight = quotedExpressionReader.BorderLeft;
            }
        }

        public char EscapeChar
        {
            get
            {
                return quotedExpressionReader.EscapeChar;
            }
            set
            {
                quotedExpressionReader.EscapeChar = value;
            }
        }

        #endregion

        #region Public Methods

        public bool Read(IExpressionSource source, out IExpression expression)
        {
            IExpressionList<IExpression> result = new ExpressionList<IExpression>(Name);

            StringBuilder currentField = new StringBuilder();
            int index = 0;
            bool quotesPossible = true;

            while (source.Index < source.Length)
            {
                string space = ReadSpace(source);
                char c = source.GetChar();

                // Quoted Strings
                bool quoted = false;
                if (c == QuoteChar && quotesPossible)
                {
                    string quotedText;
                    if (ReadQuotedText(source, out quotedText))
                    {
                        currentField.Append(quotedText);
                        quotesPossible = false;
                        ReadSpace(source);
                        quoted = true;
                    }
                }

                if (quoted)
                    continue;

                // Leerraum
                currentField.Append(space);

                // Zeilenende
                bool lineEnd = false;
                while (c == '\r' || c == '\n')
                {
                    lineEnd = true;
                    source.Read();
                    c = source.GetChar();
                }

                if (lineEnd)
                    break;

                // Delimiter
                bool delimited = false;
                foreach (string delimiter in Delimiters)
                {
                    if (source.GetString(0, delimiter.Length) == delimiter)
                    {
                        result.Add(new ValueExpression(index++.ToString(), currentField.ToString()));
                        currentField.Clear();
                        source.Read(delimiter.Length);
                        quotesPossible = true;
                        delimited = true;
                    }
                }

                if (delimited)
                    continue;

                // Sonstige Inhalte
                currentField.Append(c);
                quotesPossible = false;

                source.Read();
            }

            if (result.Any())
            {
                result.Add(new ValueExpression(index++.ToString(), currentField.ToString()));
                expression = result;
                return true;
            }

            expression = null;
            return false;
        }

        public bool Read(IExpressionSource source, out string [] fields)
        {
            IExpression fieldsExpression;
            if (Read(source, out fieldsExpression))
            {
                fields = ((IExpressionList<IExpression>)fieldsExpression).Select(exp => ((IValueExpression)exp).Value).ToArray();
                return true;
            }

            fields = null;
            return false;
        }

        #endregion

        #region Private Methods

        private string ReadSpace(IExpressionSource source)
        {
            IExpression expression;
            if (spaceExpressionReader.Read(source, out expression))
                return ((IValueExpression)expression).Value;

            return string.Empty;
        }

        private bool ReadQuotedText(IExpressionSource source, out string quotedText)
        {
            IExpression expression;
            if (quotedExpressionReader.Read(source, out expression))
            {
                quotedText = ((IValueExpression)expression).Value.Replace("\r", "").Replace("\n", Environment.NewLine);
                return true;
            }

            quotedText = null;
            return false;
        }

        #endregion
    }
}