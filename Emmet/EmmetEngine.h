#include "EditorProxy.h"

#pragma once

using namespace v8;

enum EmmetResult
{
    EmmetResult_OK,

    EmmetResult_NoActiveDocument,

    EmmetResult_DocumentFormatNotSupported,

    EmmetResult_UnexpectedError
};

class CEmmetEngine
{
public:
    CEmmetEngine();
    ~CEmmetEngine(void);

    EmmetResult Initialize(_DTE* pDTE, PCWSTR szEngineScriptPath);
    EmmetResult ExpandAbbreviation();
    EmmetResult WrapWithAbbreviation(const char* szAbbreviation, UINT nchAbbreviation);
    EmmetResult ToggleComment();
    EmmetResult RemoveTag();
    EmmetResult MergeLines();

    CComBSTR GetLastError();

private:
    EmmetResult ReadAndCompileEngineScript(PCWSTR szEngineScriptPath);
	EmmetResult RunAction(Persistent<Function>* func, EmmetAction actionCode, const char* param = NULL);
    VOID FormatExceptionMessage(TryCatch* exceptionInfo);

private:
    CAutoPtr<CEditorProxy> m_editorProxy;

	Isolate* m_isolate;
    Persistent<Context> m_context;

	Persistent<Function> m_expandAbbreviationFunc;
	Persistent<Function> m_wrapWithAbbreviationFunc;
	Persistent<Function> m_toggleCommentFunc;
	Persistent<Function> m_mergeLinesFunc;
    
	CComPtr<_DTE> m_DTE;

    CComBSTR m_lastError;
};