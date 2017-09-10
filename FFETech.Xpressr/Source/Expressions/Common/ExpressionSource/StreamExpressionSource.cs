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

namespace FFETech.Xpressr.Expressions
{
    public class StreamExpressionSource : IExpressionSource
    {
        #region Fields

        private const int bufferLength = 10000;

        private StreamReader streamReader;
        private Dictionary<object, long> bookmarks;
        private string buffer;
        private long minimumBufferStart;
        private long bufferStart;
        private long bufferEnd;
        private long streamLength;
        private long currentStreamPosition;

        #endregion

        #region Constructors

        public StreamExpressionSource(Stream stream, Encoding encoding)
        {
            streamReader = new StreamReader(stream, encoding);
            streamLength = stream.Length - encoding.GetPreamble().Length;
            ProvideBuffer(0);
        }

        public StreamExpressionSource(Stream stream)
        {
            streamReader = new StreamReader(stream);
            streamLength = stream.Length - streamReader.CurrentEncoding.GetPreamble().Length;
            ProvideBuffer(0);
        }

        #endregion

        #region Properties

        public long Index
        {
            get;
            private set;
        }

        public long Length
        {
            get;
            private set;
        }

        #endregion

        #region Public Methods

        public bool Read(int count = 1)
        {
            ProvideBuffer(count);

            if (Index >= Length)
                return false;

            Index += count;

            if (bookmarks == null || bookmarks.Count == 0)
                minimumBufferStart = Index;

            return true;
        }

        public char GetChar(int offset = 0)
        {
            if (Index + offset >= Length)
                return (char)0;

            ProvideBuffer(offset);
            return buffer[(int)(Index - bufferStart) + offset];
        }

        public string GetString(int startOffset, int length)
        {
            if (Index + startOffset >= Length)
                return string.Empty;

            ProvideBuffer(startOffset + length);
            return buffer.Substring((int)(Index - bufferStart) + startOffset, length);
        }

        public IEnumerable<string> BufferString(int bufferSize)
        {
            string result = GetString(0, (int)(bufferEnd - Index));

            if (result.Length == 0)
                result = GetString(0, bufferSize);

            int length = 0;

            while (result.Length > length)
            {
                length = result.Length;
                yield return result;
                result = GetString(0, length + bufferSize);
            }
        }

        public void SetBookmark(object owner)
        {
            if (bookmarks == null)
                bookmarks = new Dictionary<object, long>();
            bookmarks[owner] = Index;
        }

        public void GotoBookmark(object owner)
        {
            long bookmark;
            if (bookmarks == null || !bookmarks.TryGetValue(owner, out bookmark))
                throw new InvalidOperationException("Invalid bookmark");
            Index = bookmark;
        }

        public void ClearBookmark(object owner)
        {
            if (bookmarks == null || !bookmarks.Remove(owner))
                throw new InvalidOperationException("Invalid bookmark");

            minimumBufferStart = bookmarks.Count > 0 ? bookmarks.Values.Min() : Index;
        }

        #endregion

        #region Private Methods

        private void ProvideBuffer(long offset)
        {
            if (bufferEnd <= Index + offset)
            {
                char[] charBuffer = new char[bufferLength];
                int readCount = streamReader.ReadBlock(charBuffer, 0, bufferLength);

                if (bufferStart < minimumBufferStart)
                {
                    buffer = buffer.Substring((int)(minimumBufferStart - bufferStart));
                    bufferStart = minimumBufferStart;
                }

                buffer = buffer + new string(charBuffer, 0, readCount);
                bufferEnd += readCount;
                currentStreamPosition += streamReader.CurrentEncoding.GetByteCount(charBuffer, 0, readCount);

                if (currentStreamPosition > 0)
                    Length = streamLength * bufferEnd / currentStreamPosition;
                else
                    Length = bufferEnd;
            }
        }

        #endregion
    }
}