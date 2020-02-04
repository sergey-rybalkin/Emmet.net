using System.Text;

namespace Emmet.Tests.Helpers
{
    /// <summary>
    /// Emmet editor callbacks interface implementation for unit testing.
    /// </summary>
    public class EditorStub : ICodeEditor
    {
        private const char CursorMarker = '|';

        private const char SelectionStartMarker = '[';

        private const char SelectionEndMarker = ']';

        private readonly string _syntax;

        private string _content;

        private int _selectionStart;

        private int _selectionEnd;

        private int _currentLineStart;

        private int _currentLineEnd;

        public string Content
        {
            get { return _content; }
        }

        public string UserInput { get; set; }

        public string AbbreviationPrefix { get; set; }

        private EditorStub(string syntax)
        {
            _syntax = syntax;
        }

        public static EditorStub BuildFromTemplate(string contentTemplate, string syntax)
        {
            var retVal = new EditorStub(syntax);
            retVal.ParseTemplate(contentTemplate);

            return retVal;
        }

        private void ParseTemplate(string contentTemplate)
        {
            var content = new StringBuilder(contentTemplate.Length);
            int offset = 0;

            // Cleanup code from markers and save their positions.
            for (int i = 0; i < contentTemplate.Length; i++)
            {
                char ch = contentTemplate[i];

                if (ch is CursorMarker)
                    _selectionStart = i - offset++;
                else if (ch is SelectionStartMarker)
                    _selectionStart = i - offset++;
                else if (ch is SelectionEndMarker)
                    _selectionEnd = i - offset++;
                else
                    content.Append(ch);
            }

            _content = content.ToString();

            if (_selectionStart is 0)
                _selectionStart = _content.Length;

            if (_selectionEnd is 0)
                _selectionEnd = _selectionStart;

            // Calculate current line and its range
            int index = 0, lineStart = 0, lineEnd = 0;
            do
            {
                if (_content[index] is '\n')
                {
                    if (index < _selectionStart)
                        lineStart = index + 1;
                    else
                    {
                        lineEnd = index;
                        break;
                    }
                }
            }
            while (++index < _content.Length);

            if (lineEnd is 0)
                lineEnd = _content.Length;

            _currentLineStart = lineStart;
            _currentLineEnd = lineEnd;
        }

        public string GetCurrentLine()
        {
            return _content.Substring(_currentLineStart, _currentLineEnd - _currentLineStart);
        }

        public int GetCaretPosColumn()
        {
            return _selectionStart - _currentLineStart;
        }

        public string GetContentTypeInActiveBuffer()
        {
            return _syntax;
        }

        public void ReplaceCurrentLine(string newContent)
        {
            _content = _content.Remove(_currentLineStart, _currentLineEnd - _currentLineStart)
                               .Insert(_currentLineStart, newContent);
        }

        public string Prompt() => UserInput;

        public string GetSelection()
        {
            return _content.Substring(_selectionStart, _selectionEnd - _selectionStart);
        }

        public void ReplaceSelection(string newContent)
        {
            _content = _content.Remove(_selectionStart, _selectionEnd - _selectionStart)
                               .Insert(_selectionStart, newContent);
        }
    }
}