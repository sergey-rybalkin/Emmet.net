using System;
using System.Text;
using Emmet.Engine;

namespace Emmet.Tests.Helpers
{
    /// <summary>
    /// Emmet editor callbacks interface implementation for unit testing.
    /// </summary>
    public class EditorStub : IEmmetEditor
    {
        private const char CursorMarker = '|';

        private const char SelectionStartMarker = '[';

        private const char SelectionEndMarker = ']';

        private readonly string _syntax;

        private string _content;

        private int _selectionStart;

        private int _selectionEnd;

        private Range _currentLine;

        public string Content
        {
            get { return _content; }
        }

        private EditorStub(string syntax)
        {
            _syntax = syntax;
        }

        public static EditorStub BuildFromTemplate(string contentTemplate, string syntax)
        {
            EditorStub retVal = new EditorStub(syntax);
            retVal.ParseTemplate(contentTemplate);

            return retVal;
        }

        private void ParseTemplate(string contentTemplate)
        {
            StringBuilder content = new StringBuilder(contentTemplate.Length);
            int offset = 0;

            // Cleanup code from markers and save their positions.
            for (int i = 0; i < contentTemplate.Length; i++)
            {
                char ch = contentTemplate[i];

                if (CursorMarker == ch)
                    _selectionStart = i - offset++;
                else if (SelectionStartMarker == ch)
                    _selectionStart = i - offset++;
                else if (SelectionEndMarker == ch)
                    _selectionEnd = i - offset++;
                else
                    content.Append(ch);
            }

            _content = content.ToString();

            if (0 == _selectionStart)
                _selectionStart = _content.Length;

            if (0 == _selectionEnd)
                _selectionEnd = _selectionStart;            

            // Calculate current line and its range
            int index = 0, lineStart = 0, lineEnd = 0;
            do
            {
                if ('\n' == _content[index])
                {
                    if (index < _selectionStart)
                        lineStart = index;
                    else
                        lineEnd = index;
                }
            }
            while (++index < _content.Length);
            
            if (0 == lineEnd)
                lineEnd = _content.Length - 1;

            _currentLine = new Range(lineStart, lineEnd);
        }

        public void CreateSelection(int start, int end)
        {
        }

        public void FormatRegion(int startPosition = -1, int endPosition = 0)
        {
        }

        public int GetCaretPosition()
        {
            return _selectionStart;
        }

        public string GetContent()
        {
            return _content;
        }

        public string GetContentTypeInActiveBuffer()
        {
            return _syntax;
        }

        public string GetCurrentLine()
        {
            return _content.Substring(_currentLine.Start, _currentLine.End - _currentLine.Start);
        }

        public Range GetCurrentLineRange()
        {
            return _currentLine;
        }

        public string GetSelection()
        {
            return _content.Substring(_selectionStart, _selectionEnd - _selectionStart);
        }

        public Range GetSelectionRange()
        {
            return new Range(_selectionStart, _selectionEnd);
        }

        public string Prompt()
        {
            return "div";
        }

        public void ReplaceContentRange(string newContent, int startPosition = -1, int endPosition = 0)
        {
            if (startPosition is -1)
                _content = newContent;
            else if (endPosition is 0)
                _content = _content.Insert(startPosition, newContent);
            else
                _content = _content.Remove(startPosition, endPosition - startPosition)
                                   .Insert(startPosition, newContent);
        }

        public void SetCaretPosition(int position)
        {
        }

        public void TrackTabStops(Range[] tabStops, int[] tabStopGroups)
        {
        }
    }
}