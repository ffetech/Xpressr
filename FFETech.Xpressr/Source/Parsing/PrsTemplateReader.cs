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

using FFETech.Xpressr.Expressions;

namespace FFETech.Xpressr.Parsing
{
    internal class PrsTemplateReader : ExpressionVisitor, IPrsExpressionVisitor
    {
        #region Nested Types

        private class EndExpressionTarget : IPrsExpressionTarget
        {
            #region Constructors

            public EndExpressionTarget(PrsRange target)
            {
                Target = target;
            }

            #endregion

            #region Explicit Members

            bool IPrsExpressionTarget.GetDefaultProperty(int index, out string propertyName)
            {
                propertyName = string.Empty;
                return false;
            }

            void IPrsExpressionTarget.AssignProperty(string propertyName, IExpression expression)
            {
                throw new Exception("End expression have no properties");
            }

            IPrsExpressionTarget IPrsExpressionTarget.CreateChildTarget(string propertyName)
            {
                throw new Exception("End expression have no properties");
            }

            #endregion

            #region Properties

            public PrsRange Target
            {
                get;
                private set;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Stack<PrsRange> composite = new Stack<PrsRange>();

        private PrsRange currentRange;

        #endregion

        #region Constructors

        private PrsTemplateReader(PrsTemplate document)
        {
            composite.Push(document);
            currentRange = document;

            RegisterExpressionReader(new EmbraceExpressionReader("Comment", "{*", "*}"));
            RegisterExpressionReader(new CprMarkupExpressionReader("Default", this));
        }

        #endregion

        #region Explicit Members

        Type IPrsExpressionVisitor.GetElementType(string name)
        {
            return currentRange.GetElementType(name);
        }

        IPrsExpressionTarget IPrsExpressionVisitor.CreateElementTarget(Type elementType)
        {
            return currentRange.CreateChild(elementType);
        }

        IPrsExpressionTarget IPrsExpressionVisitor.CreateElementEndTarget(Type elementType)
        {
            if (currentRange.GetType() != elementType)
                throw new Exception("End of range don't fits to the range start");
            return new EndExpressionTarget(currentRange);
        }

        #endregion

        #region Public Static Methods

        public static void Read(PrsTemplate document, string template)
        {
            new PrsTemplateReader(document).Visit(template);
        }

        #endregion

        #region Protected Methods

        protected override void VisitText(string text)
        {
            currentRange.AddChild(new PrsContentElement(currentRange, text));
        }

        protected override void VisitExpression(IExpression expression)
        {
            IValueExpression commentExpression = expression as IValueExpression;
            if (commentExpression != null && expression.Name == "Comment")
            {
                VisitText(commentExpression.Value);
                return;
            }

            IPrsExpressionTarget target = ((CprMarkupExpression)expression).Target;

            if (target is PrsElement)
            {
                currentRange.AddChild((PrsElement)target);

                PrsRange newRange = target as PrsRange;
                if (newRange != null)
                {
                    composite.Push((PrsRange)target);
                    currentRange = newRange;
                }
            }
            else if (target is EndExpressionTarget)
            {
                if (currentRange != ((EndExpressionTarget)target).Target)
                    throw new InvalidOperationException("Invalid end target");

                composite.Pop();
                currentRange = composite.Peek();
            }
            else throw new InvalidOperationException("Unknown target");
        }

        #endregion
    }
}