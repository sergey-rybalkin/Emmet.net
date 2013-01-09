#include "EditorProxy.h"
#include "FileProxy.h"

#pragma once

using namespace v8;

enum EmmetResult
{
    EmmetResult_OK,

    EmmetResult_NoActiveDocument,

    EmmetResult_DocumentFormatNotSupported,

    EmmetResult_UnexpectedError,

    EmmetResult_CompilationFailed,

    EmmetResult_NotInitialized
};

class CEmmetEngine
{
public:
    CEmmetEngine();
    ~CEmmetEngine(void);

    EmmetResult Initialize(_DTE* pDTE, PCWSTR szEngineScript);
    EmmetResult ExpandAbbreviation();
    EmmetResult WrapWithAbbreviation(const char* szAbbreviation, UINT nchAbbreviation);
    EmmetResult ToggleComment();
    EmmetResult RemoveTag();
    EmmetResult MergeLines();
    EmmetResult UpdateImageSize();
private:

    EmmetResult RunInternal(const char* action, EmmetAction actionCode);

    CEditorProxy* m_pEditorProxy;
    CFileProxy* m_pFileProxy;
    HandleScope m_handleScope;
    Persistent<v8::Context> m_Context;
    _DTE* m_pDTE;
};