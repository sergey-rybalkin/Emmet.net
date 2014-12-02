#pragma once

enum EmmetAction
{
	EmmetAction_ExpandAbbreviation,

	EmmetAction_WrapWithAbbreviation,

	EmmetAction_ToggleComment,

	EmmetAction_MergeLines
};

enum EmmetSyntax
{
    EmmetSyntax_Html,

    EmmetSyntax_Css,

    EmmetSyntax_Scss
};

struct CallbackOptions
{
	Document* m_doc;
	TextDocument* m_textDoc;
	TextSelection* m_selection;
	EmmetSyntax m_syntax;
	EmmetAction m_curAction;
};

using namespace v8;

void EditorUpdateExecutionOptions(CallbackOptions options);

void EditorGetSelectionRange(const FunctionCallbackInfo<Value>& args);

void EditorCreateSelection(const FunctionCallbackInfo<Value>& args);

void EditorGetCurrentLineRange(const FunctionCallbackInfo<Value>& args);

void EditorGetCaretPos(const FunctionCallbackInfo<Value>& args);

void EditorSetCaretPos(const FunctionCallbackInfo<Value>& args);

void EditorGetCurrentLine(const FunctionCallbackInfo<Value>& args);

void EditorReplaceContent(const FunctionCallbackInfo<Value>& args);

void EditorGetContent(const FunctionCallbackInfo<Value>& args);

void EditorGetSelection(const FunctionCallbackInfo<Value>& args);

void EditorGetSyntax(const FunctionCallbackInfo<Value>& args);

void EditorGetProfileName(const FunctionCallbackInfo<Value>& args);

void EditorPrompt(const FunctionCallbackInfo<Value>& args);