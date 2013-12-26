#include "stdafx.h"
#include "EditorProxy.h"
#include <memory.h>

using namespace v8;

CEditorProxy::CEditorProxy(void)
{
}

CEditorProxy::~CEditorProxy(void)
{
}

Handle<ObjectTemplate> CEditorProxy::GetEditorInterfaceImplementation()
{
	HandleScope scope(Isolate::GetCurrent());
    Handle<ObjectTemplate> editorProxy = ObjectTemplate::New();

    editorProxy->Set(String::New("getSelectionRange"), FunctionTemplate::New(EditorGetSelectionRange));
    editorProxy->Set(String::New("createSelection"), FunctionTemplate::New(EditorCreateSelection));
    editorProxy->Set(String::New("getProfileName"), FunctionTemplate::New(EditorGetProfileName));
    editorProxy->Set(String::New("getCurrentLineRange"), FunctionTemplate::New(EditorGetCurrentLineRange));
    editorProxy->Set(String::New("getCaretPos"), FunctionTemplate::New(EditorGetCaretPos));
    editorProxy->Set(String::New("setCaretPos"), FunctionTemplate::New(EditorSetCaretPos));
    editorProxy->Set(String::New("getCurrentLine"), FunctionTemplate::New(EditorGetCurrentLine));
    editorProxy->Set(String::New("replaceContent"), FunctionTemplate::New(EditorReplaceContent));
    editorProxy->Set(String::New("getContent"), FunctionTemplate::New(EditorGetContent));
    editorProxy->Set(String::New("getSyntax"), FunctionTemplate::New(EditorGetSyntax));
    editorProxy->Set(String::New("prompt"), FunctionTemplate::New(EditorPrompt));
    editorProxy->Set(String::New("getSelection"), FunctionTemplate::New(EditorGetSelection));

	return scope.Close(editorProxy);
}

BOOL CEditorProxy::Initialize(Document* pDoc, TextDocument* pTextDoc, TextSelection* pSelection, EmmetAction action)
{
    CComBSTR bstrLang;
    CComBSTR bstrName;
    pDoc->get_Language(&bstrLang);
    pDoc->get_Name(&bstrName);

	CallbackOptions options;
	options.m_curAction = action;
	options.m_doc = pDoc;

	// We need to distinguish HTML and CSS syntaxes only, it doesn't really matter whether CSS is actually
	// LESS or SASS document
	options.m_isHtml = bstrLang == "HTMLX" || bstrLang == "HTML";
	options.m_selection = pSelection;
	options.m_textDoc = pTextDoc;

	EditorUpdateExecutionOptions(options);

    return TRUE;
}