using System;
using System.Linq;
using Emmet.Engine.ChakraInterop;
using static Emmet.Diagnostics.Tracer;

namespace Emmet.Engine
{
    /// <summary>
    /// Implementation of IEmmetEditor interface for JavaScript engine. See
    /// https://github.com/emmetio/emmet/blob/master/lib/interfaces/IEmmetEditor.js for details.
    /// </summary>
    public class EmmetEditorCallbacks
    {
        private IEmmetEditor _editor = null;

        private string _syntax;

        /// <summary>
        /// Attaches this instance to the given code editor.
        /// </summary>
        /// <param name="editor">Editor to attach to.</param>
        public bool Attach(IEmmetEditor editor)
        {
            _editor = editor;

            // Detect syntax at the caret position
            string contentType = editor.GetContentTypeInActiveBuffer();
            if (string.IsNullOrEmpty(contentType))
                return false;

            // css, less or scss
            if (contentType.EndsWith(@"ss"))
            {
                _syntax = contentType;
            }
            else if (@"htmlx" == contentType || contentType.StartsWith("razor"))
            {
                _syntax = @"html";
            }
            else
            {
                Trace($"Syntax {contentType} is not supported");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Detaches this instance from the editor window, cleans up any internal caches.
        /// </summary>
        public void Detach()
        {
            _editor = null;
            _syntax = null;
        }

        /// <summary>
        /// JavaScript callback. Returns character indexes of selected text: object with <code>start</code>
        /// and <code>end</code> properties.If there's no selection, should return object with
        /// <code>start</code> and <code>end</code> properties referring to current caret position.
        /// </summary>
        public JavaScriptValue GetSelectionRange(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            var selection = _editor.GetSelectionRange();
            JavaScriptValue retVal = JavaScriptValue.CreateObject();
            JavaScriptValue start = JavaScriptValue.FromInt32(selection.Start);
            JavaScriptValue end = JavaScriptValue.FromInt32(selection.End);
            retVal.SetProperty("start", start);
            retVal.SetProperty("end", end);

            return retVal;
        }

        /// <summary>
        /// JavaScript callback. Creates selection from <code>start</code> to <code>end</code> character
        /// indexes. If <code>end</code> is omitted, this method should place caret and <code>start</code>
        /// index.
        /// </summary>
        public JavaScriptValue CreateSelection(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            if (3 == argumentCount)
            {
                int start = arguments[1].ToInt32();
                int end = arguments[2].ToInt32();
                _editor.CreateSelection(start, end);
            }
            else
            {
                return SetCarretPos(callee, isConstructCall, arguments, argumentCount, callbackData);
            }

            return JavaScriptValue.True;
        }

        /// <summary>
        /// JavaScript callback. Returns current line's start and end indexes as object with
        /// <code>start</code> and <code>end</code> properties.
        /// </summary>
        public JavaScriptValue GetCurrentLineRange(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            Range line = _editor.GetCurrentLineRange();

            JavaScriptValue retVal = JavaScriptValue.CreateObject();
            JavaScriptValue start = JavaScriptValue.FromInt32(line.Start);
            JavaScriptValue end = JavaScriptValue.FromInt32(line.End);
            retVal.SetProperty("start", start);
            retVal.SetProperty("end", end);

            return retVal;
        }

        /// <summary>
        /// JavaScript callback. Returns current caret position.
        /// </summary>
        public JavaScriptValue GetCarretPos(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            return JavaScriptValue.FromInt32(_editor.GetCaretPosition());
        }

        /// <summary>
        /// JavaScript callback. Set new caret position.
        /// </summary>
        public JavaScriptValue SetCarretPos(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            _editor.SetCaretPosition(arguments[1].ToInt32());

            return JavaScriptValue.True;
        }

        /// <summary>
        /// JavaScript callback. Returns the content of the current line.
        /// </summary>
        public JavaScriptValue GetCurrentLine(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            string txt = _editor.GetCurrentLine();

            return JavaScriptValue.FromString(txt);
        }

        /// <summary>
        /// JavaScript callback. Replace editor's content or it's part (from <code>start</code> to
        /// <code>end</code> index). If <code>value</code> contains <code>caret_placeholder</code>, the editor
        /// will put caret into this position. If you skip <code>start</code> and <code>end</code> arguments,
        /// the whole target's content will be replaced with <code>value</code>.
        /// If you pass <code>start</code> argument only, the <code>value</code> will be placed at
        /// <code>start</code> string index of current content.
        /// If you pass <code>start</code> and <code>end</code> arguments, the corresponding substring of
        /// current target's content will be replaced with <code>value</code>.
        /// </summary>
        public JavaScriptValue ReplaceContent(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            string rawContent = arguments[1].ToString();
            int regionStart = argumentCount > 2 ? arguments[2].ToInt32() : -1;
            int regionLength = argumentCount > 3 ? arguments[3].ToInt32() - regionStart : 0;
            bool indentContent = 5 == argumentCount ? arguments[4].ToBoolean() : true;

            Trace($"Received new content for the editor: {rawContent}");

            // Extract tab stops placeholders from the specified content.
            var tabStops = TabStopsParser.ParseContent(arguments[1]);

            _editor.ReplaceContentRange(tabStops.Content, regionStart, regionStart + regionLength);

            if (null != tabStops.TabStops)
            {
                Range[] tabStopRanges = tabStops.TabStops;

                // Tab stop offsets are relative to the newly generated content ranges, we need to convert
                // them to the document-wide offsets.
                if (regionStart > 0)
                {
                    tabStopRanges = tabStopRanges.Select(
                        item => new Range(item.Start + regionStart, item.End + regionStart)).ToArray();
                }

                _editor.TrackTabStops(tabStopRanges, tabStops.TabStopGroups);
            }

            if (indentContent)
                _editor.FormatRegion(regionStart, regionStart + tabStops.Content.Length);

            return JavaScriptValue.True;
        }

        /// <summary>
        /// JavaScript callbacks. Returns the content of the current editor window.
        /// </summary>
        public JavaScriptValue GetContent(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            return JavaScriptValue.FromString(_editor.GetContent());
        }

        /// <summary>
        /// JavaScript callback. Returns current editor's syntax mode.
        /// </summary>
        public JavaScriptValue GetSyntax(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            return JavaScriptValue.FromString(_syntax);
        }

        /// <summary>
        /// JavaScript callback. Returns current output profile name (see profile module). In most cases,
        /// this method should return <code>null</code> and let Emmet guess best profile name for current
        /// syntax and user data. In case you’re using advanced editor with access to syntax scopes (like
        /// Sublime Text 2), you can return syntax name for current scope. For example, you may return
        /// `line` profile when editor caret is inside string of programming language.
        /// </summary>
        public JavaScriptValue GetProfileName(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            // Let Emmet engine detect profile
            return JavaScriptValue.Null;
        }

        /// <summary>
        /// JavaScript callback. Asks user to enter something.
        /// </summary>
        public JavaScriptValue Prompt(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            string input = _editor.Prompt();
            if (string.IsNullOrWhiteSpace(input))
                return JavaScriptValue.Null;

            return JavaScriptValue.FromString(input);
        }

        /// <summary>
        /// JavaScript callback. Returns current selection.
        /// </summary>
        public JavaScriptValue GetSelection(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            string selection = _editor.GetSelection();
            if (string.IsNullOrEmpty(selection))
                return JavaScriptValue.FromString(string.Empty);

            return JavaScriptValue.FromString(selection);
        }

        /// <summary>
        /// JavaScript callback. Returns current editor's file path.
        /// </summary>
        public JavaScriptValue GetFilePath(
            JavaScriptValue callee,
            bool isConstructCall,
            JavaScriptValue[] arguments,
            ushort argumentCount,
            IntPtr callbackData)
        {
            // As of version 1.3 this callback is required only for actions with external images that we don't
            // support.
            return JavaScriptValue.Null;
        }
    }
}