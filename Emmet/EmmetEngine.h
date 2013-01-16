#include "EditorProxy.h"
#include "FileProxy.h"

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
    EmmetResult UpdateImageSize();

    CComBSTR GetLastError();

private:
    EmmetResult ReadAndCompileEngineScript(PCWSTR szEngineScriptPath);
    EmmetResult RunAction(const char* action, EmmetAction actionCode);
    VOID FormatExceptionMessage(TryCatch* exceptionInfo);

private:
    CAutoPtr<CEditorProxy> m_editorProxy;
    CAutoPtr<CFileProxy> m_fileProxy;

    Persistent<v8::Context> m_Context;
    HandleScope m_handleScope;
    CComPtr<_DTE> m_DTE;

    CComBSTR m_lastError;
};