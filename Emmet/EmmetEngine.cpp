#include "stdafx.h"
#include "EmmetEngine.h"
#include "FileProxy.h"

using namespace v8;

CEmmetEngine::CEmmetEngine()
{
}

EmmetResult CEmmetEngine::Initialize(_DTE* pDTE, PCWSTR szEngineScriptPath)
{
    m_DTE.Attach(pDTE);

    Handle<ObjectTemplate> global = ObjectTemplate::New();

    m_editorProxy.Attach(new CEditorProxy());
    m_fileProxy.Attach(new CFileProxy());

    m_editorProxy->Register(global);
    m_fileProxy->Register(global);

    m_Context = Context::New(NULL, global);
    m_Context->Enter();
    m_editorProxy->SetContext(m_Context);

    return ReadAndCompileEngineScript(szEngineScriptPath);
}

CEmmetEngine::~CEmmetEngine(void)
{
    m_Context->Exit();
    m_Context.Dispose();
}

EmmetResult CEmmetEngine::ExpandAbbreviation()
{
    RunAction("actionExpandAbbreviation()", EmmetAction_ExpandAbbreviation);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::WrapWithAbbreviation(const char* szAbbreviation, UINT nchAbbreviation)
{
    UINT bufSize = nchAbbreviation + 32;

    char* szCmd = (char*)HeapAlloc(GetProcessHeap(), 0, bufSize);

    StringCchPrintfA(szCmd, bufSize, "actionWrapWithAbbreviation('%s')", szAbbreviation);

    RunAction(szCmd, EmmetAction_WrapWithAbbreviation);

    HeapFree(GetProcessHeap(), 0, szCmd);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::ToggleComment()
{
    RunAction("actionToggleComment()", EmmetAction_ToggleComment);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::RemoveTag()
{
    RunAction("actionRemoveTag()", EmmetAction_RemoveTag);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::MergeLines()
{
    return RunAction("actionMergeLines()", EmmetAction_MergeLines);
}

EmmetResult CEmmetEngine::UpdateImageSize()
{
    return RunAction("actionUpdateImageSize()", EmmetAction_UpdateImageSize);
}

EmmetResult CEmmetEngine::RunAction(const char* action, EmmetAction actionCode)
{
    CComPtr<Document> pActiveDoc;
    CComPtr<IDispatch> pDisp;
    CComPtr<TextDocument> pTextDoc;
    CComPtr<TextSelection> pSelection;

    m_DTE->get_ActiveDocument(&pActiveDoc);
    if (NULL == pActiveDoc)
        return EmmetResult_NoActiveDocument;
    pActiveDoc->Object(CComBSTR("TextDocument"), &pDisp);
    pDisp->QueryInterface(__uuidof(TextDocument), (LPVOID*)&pTextDoc);
    pTextDoc->get_Selection(&pSelection);

    if (!m_editorProxy->Initialize(pActiveDoc, pTextDoc, pSelection, actionCode))
        return EmmetResult_DocumentFormatNotSupported;

    TryCatch try_catch;
    HandleScope handleScope;

    // Create a string containing the JavaScript source code.
    Handle<String> source = String::New(action);

    // Compile the source code.
    Handle<Script> script = Script::Compile(source);
  
    // All content manipulations will be run in a single undo context so that they can be reverted all at once
    CComPtr<UndoContext> undoContext;
    m_DTE->get_UndoContext(&undoContext);
    undoContext->Open(CComBSTR("Emmet"));

    // Run the script to get the result.
    Handle<Value> result = script->Run();
    undoContext->Close();

    if (result.IsEmpty())
    {
        FormatExceptionMessage(&try_catch);

        return EmmetResult_UnexpectedError;
    }

    if (result->IsBoolean() || result->IsBooleanObject())
    {
        bool bResult = result->BooleanValue();
        if (!bResult)
        {
            m_lastError.Append(L"Engine reported an error while trying to execute an action.");

            return EmmetResult_UnexpectedError;
        }
    }

    return EmmetResult_OK;
}

CComBSTR CEmmetEngine::GetLastError()
{
    CComBSTR retVal;
    m_lastError.CopyTo(&retVal);
    m_lastError.Empty();

    return retVal;
}

EmmetResult CEmmetEngine::ReadAndCompileEngineScript(PCWSTR szEngineScriptPath)
{
    TryCatch try_catch;
    CAtlFile scriptFile;
    HRESULT hr = scriptFile.Create(szEngineScriptPath,
                                   GENERIC_READ,
                                   FILE_SHARE_READ,
                                   OPEN_EXISTING,
                                   FILE_FLAG_SEQUENTIAL_SCAN);
    if (FAILED(hr))
    {
        // cannot open engine.js - update last error message and exit
        m_lastError.Append(L"Cannot read engine script");

        LPTSTR errorText = NULL;
        FormatMessage(
            FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_IGNORE_INSERTS,
            NULL,
            hr,
            MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
            (LPTSTR)&errorText,
            0,
            NULL);

        if ( NULL != errorText )
        {
            m_lastError.Append(L" - ");
            m_lastError.Append(errorText);
            LocalFree(errorText);
            errorText = NULL;
        }

        return EmmetResult_UnexpectedError;
    }

    ULONGLONG len;
    scriptFile.GetSize(len);
    CAutoPtr<char> fileContent(new char[(DWORD)len + 1]);
    scriptFile.Read(fileContent.m_p, (int)len);
    fileContent.m_p[len] = '\0';

    Handle<Script> script = Script::Compile(String::New(fileContent));

    if (script.IsEmpty())
    {
        m_lastError.Append(L"Script compilation failed.");
        return EmmetResult_UnexpectedError;
    }
    else
    {
        Handle<Value> result = script->Run();
        if (result.IsEmpty())
        {
            // Remember errors that happened during execution.
            FormatExceptionMessage(&try_catch);

            return EmmetResult_UnexpectedError;
        }
    }

    return EmmetResult_OK;
}

VOID CEmmetEngine::FormatExceptionMessage(TryCatch* exceptionInfo)
{
    m_lastError.Append(L"Emmet JavaScript error: \n");

    HandleScope handle_scope;
    String::Utf8Value exception(exceptionInfo->Exception());
    Handle<Message> message = exceptionInfo->Message();
    if (message.IsEmpty())
    {
        // V8 didn't provide any extra information about this error; just
        // print the exception.
        m_lastError.Append(*exception);
    }
    else
    {
        CAtlStringA buf;

        // Print (filename):(line number): (message).
        // (line of source code)
        // (stack trace)
        String::Utf8Value filename(message->GetScriptResourceName());
        int linenum = message->GetLineNumber();
        String::Utf8Value sourceline(message->GetSourceLine());

        buf.Format("%s:%i: %s\n%s\n", *filename, linenum, *exception, *sourceline);

        m_lastError.Append(buf.GetBuffer());
    }
}