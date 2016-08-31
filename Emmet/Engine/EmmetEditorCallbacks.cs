using System.Linq;
using Emmet.Diagnostics;
using V8.Net;

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
            else if (contentType == @"htmlx" || contentType.StartsWith("razor"))
            {
                _syntax = @"html";
            }
            else
            {
                this.TraceWarning($"Syntax {contentType} is not supported");
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
        public InternalHandle GetSelectionRange(
            V8Engine engine,
            bool isConstructCall,
            InternalHandle _this,
            params InternalHandle[] args)
        {
            var selection = _editor.GetSelectionRange();
            ObjectHandle retVal = engine.CreateObject();
            retVal.SetProperty("start", engine.CreateValue(selection.Start));
            retVal.SetProperty("end", engine.CreateValue(selection.End));

            return retVal;
        }

        /// <summary>
        /// JavaScript callback. Creates selection from <code>start</code> to <code>end</code> character
        /// indexes. If <code>end</code> is omitted, this method should place caret and <code>start</code>
        /// index.
        /// </summary>
        public InternalHandle CreateSelection(V8Engine engine,
                                              bool isConstructCall,
                                              InternalHandle _this,
                                              params InternalHandle[] args)
        {
            if (args.Length == 2)
            {
                int start = args[0].AsInt32;
                int end = args[0].AsInt32;
                _editor.CreateSelection(start, end);
            }
            else
            {
                return SetCarretPos(engine, isConstructCall, _this, args);
            }

            return engine.CreateValue(true);
        }

        /// <summary>
        /// JavaScript callback. Returns current line's start and end indexes as object with
        /// <code>start</code> and <code>end</code> properties.
        /// </summary>
        public InternalHandle GetCurrentLineRange(V8Engine engine,
                                                  bool isConstructCall,
                                                  InternalHandle _this,
                                                  params InternalHandle[] args)
        {
            var line = _editor.GetCurrentLineRange();

            ObjectHandle retVal = engine.CreateObject();
            retVal.SetProperty("start", engine.CreateValue(line.Start));
            retVal.SetProperty("end", engine.CreateValue(line.End));

            return retVal;
        }

        /// <summary>
        /// JavaScript callback. Returns current caret position.
        /// </summary>
        public InternalHandle GetCarretPos(V8Engine engine,
                                           bool isConstructCall,
                                           InternalHandle _this,
                                           params InternalHandle[] args)
        {
            return engine.CreateValue(_editor.GetCaretPosition());
        }

        /// <summary>
        /// JavaScript callback. Set new caret position.
        /// </summary>
        public InternalHandle SetCarretPos(V8Engine engine,
                                           bool isConstructCall,
                                           InternalHandle _this,
                                           params InternalHandle[] args)
        {
            _editor.SetCaretPosition(args[0].AsInt32);

            return engine.CreateValue(true);
        }

        /// <summary>
        /// JavaScript callback. Returns the content of the current line.
        /// </summary>
        public InternalHandle GetCurrentLine(V8Engine engine,
                                             bool isConstructCall,
                                             InternalHandle _this,
                                             params InternalHandle[] args)
        {
            string txt = _editor.GetCurrentLine();

            return engine.CreateValue(txt);
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
        public InternalHandle ReplaceContent(V8Engine engine,
                                             bool isConstructCall,
                                             InternalHandle _this,
                                             params InternalHandle[] args)
        {
            string rawContent = args[0].AsString;
            int regionStart = args.Length > 1 ? args[1].AsInt32 : -1;
            int regionLength = args.Length > 2 ? args[2].AsInt32 - regionStart : 0;
            bool indentContent = args.Length == 4 ? args[3].AsBoolean : true;

            this.Trace($"Received new content for the editor: {rawContent}");

            // Extract tab stops placeholders from the specified content.
            var tabStops = TabStopsParser.ParseContent(engine, rawContent);

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

            return engine.CreateValue(true);
        }

        /// <summary>
        /// JavaScript callbacks. Returns the content of the current editor window.
        /// </summary>
        public InternalHandle GetContent(V8Engine engine,
                                         bool isConstructCall,
                                         InternalHandle _this,
                                         params InternalHandle[] args)
        {
            return engine.CreateValue(_editor.GetContent());
        }

        /// <summary>
        /// JavaScript callback. Returns current editor's syntax mode.
        /// </summary>
        public InternalHandle GetSyntax(V8Engine engine,
                                        bool isConstructCall,
                                        InternalHandle _this,
                                        params InternalHandle[] args)
        {
            return engine.CreateValue(_syntax);
        }

        /// <summary>
        /// JavaScript callback. Returns current output profile name (see profile module). In most cases,
        /// this method should return <code>null</code> and let Emmet guess best profile name for current
        /// syntax and user data. In case you’re using advanced editor with access to syntax scopes (like
        /// Sublime Text 2), you can return syntax name for current scope. For example, you may return
        /// `line` profile when editor caret is inside string of programming language.
        /// </summary>
        public InternalHandle GetProfileName(V8Engine engine,
                                             bool isConstructCall,
                                             InternalHandle _this,
                                             params InternalHandle[] args)
        {
            // Let Emmet engine detect profile
            return engine.CreateNullValue();
        }

        /// <summary>
        /// JavaScript callback. Asks user to enter something.
        /// </summary>
        public InternalHandle Prompt(V8Engine engine,
                                     bool isConstructCall,
                                     InternalHandle _this,
                                     params InternalHandle[] args)
        {
            string input = _editor.Prompt();
            if (string.IsNullOrWhiteSpace(input))
                return engine.CreateNullValue();

            return engine.CreateValue(input);
        }

        /// <summary>
        /// JavaScript callback. Returns current selection.
        /// </summary>
        public InternalHandle GetSelection(V8Engine engine,
                                           bool isConstructCall,
                                           InternalHandle _this,
                                           params InternalHandle[] args)
        {
            string selection = _editor.GetSelection();
            if (string.IsNullOrEmpty(selection))
                return engine.CreateValue(string.Empty);

            return engine.CreateValue(selection);
        }

        /// <summary>
        /// JavaScript callback. Returns current editor's file path.
        /// </summary>
        public InternalHandle GetFilePath(V8Engine engine,
                                          bool isConstructCall,
                                          InternalHandle _this,
                                          params InternalHandle[] args)
        {
            // As of version 1.3 this callback is required only for actions with external images that we don't
            // support.
            return engine.CreateNullValue();
        }
    }
}