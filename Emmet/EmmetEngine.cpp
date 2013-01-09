#include "stdafx.h"
#include "EmmetEngine.h"
#include "FileProxy.h"

using namespace v8;

Handle<String> ReadFile(PCWSTR name) {
  FILE* file;
  _wfopen_s(&file, name, L"rb");
  if (file == NULL)
      return v8::Handle<v8::String>();

  fseek(file, 0, SEEK_END);
  int size = ftell(file);
  rewind(file);

  char* chars = new char[size + 1];
  chars[size] = '\0';
  for (int i = 0; i < size;) {
    int read = static_cast<int>(fread(&chars[i], 1, size - i, file));
    i += read;
  }
  fclose(file);
  v8::Handle<v8::String> result = v8::String::New(chars, size);
  delete[] chars;

  return result;
}

const PWSTR ToCString(const v8::String::Utf8Value& value)
{
    char* retVal = *value ? *value : "<string conversion failed>";
    int nBuf = value.length() + 1;
    TCHAR* buf = (PWSTR)HeapAlloc(GetProcessHeap(), 0, nBuf * sizeof(TCHAR));
    
    MultiByteToWideChar(CP_ACP, 0, *value, value.length() + 1, buf, nBuf);

    return buf;
}

void ReportException(TryCatch* try_catch)
{
    HandleScope handle_scope;
    String::Utf8Value exception(try_catch->Exception());
    PWSTR exception_string = ToCString(exception);
    Handle<Message> message = try_catch->Message();
    if (message.IsEmpty())
    {
        // V8 didn't provide any extra information about this error; just
        // print the exception.
        MessageBox(NULL, exception_string, L"Emmet JavaScript Error", MB_OK | MB_ICONERROR);
    }
    else
    {
        const int nBuf = 2048;
        TCHAR buf[nBuf];

        // Print (filename):(line number): (message).
        // (line of source code)
        // (stack trace)
        String::Utf8Value filename(message->GetScriptResourceName());
        PWSTR filename_string = ToCString(filename);
        int linenum = message->GetLineNumber();
        String::Utf8Value sourceline(message->GetSourceLine());
        PWSTR sourceline_string = ToCString(sourceline);

        StringCchPrintf(buf, nBuf, L"%s:%i: %s\n%s\n",
                                   filename_string,
                                   linenum,
                                   exception_string,
                                   sourceline_string);

        HeapFree(GetProcessHeap(), 0, filename_string);
        HeapFree(GetProcessHeap(), 0, sourceline_string);

        MessageBox(NULL, buf, L"Emmet JavaScript Error", MB_OK | MB_ICONERROR);
    }

    HeapFree(GetProcessHeap(), 0, exception_string);
}

CEmmetEngine::CEmmetEngine()
{
}

EmmetResult CEmmetEngine::Initialize(_DTE* pDTE, PCWSTR szEngineScript)
{
    m_pDTE = pDTE;

    Handle<ObjectTemplate> global = ObjectTemplate::New();
    m_pEditorProxy = new CEditorProxy();
    m_pFileProxy = new CFileProxy();
    m_pEditorProxy->Register(global);
    m_pFileProxy->Register(global);

    m_Context = Context::New(NULL, global);
    m_Context->Enter();
    m_pEditorProxy->SetContext(m_Context);

    TryCatch try_catch;
    Handle<String> scriptSource = ReadFile(szEngineScript);
    Handle<Script> script = Script::Compile(scriptSource);

    if (script.IsEmpty())
    {
        return EmmetResult_CompilationFailed;
    }
    else
    {
        Handle<Value> result = script->Run();
        if (result.IsEmpty())
        {
            // Print errors that happened during execution.
            ReportException(&try_catch);

            return EmmetResult_UnexpectedError;
        }
    }

    return EmmetResult_OK;
}

CEmmetEngine::~CEmmetEngine(void)
{
    if (NULL != m_pDTE)
    {
        m_pDTE->Release();
        m_pDTE = NULL;
    }

    delete m_pEditorProxy;
    delete m_pFileProxy;

    m_Context->Exit();
    m_Context.Dispose();
}

EmmetResult CEmmetEngine::ExpandAbbreviation()
{
    RunInternal("actionExpandAbbreviation()", EmmetAction_ExpandAbbreviation);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::WrapWithAbbreviation(const char* szAbbreviation, UINT nchAbbreviation)
{
    UINT bufSize = nchAbbreviation + 32;

    char* szCmd = (char*)HeapAlloc(GetProcessHeap(), 0, bufSize);

    StringCchPrintfA(szCmd, bufSize, "actionWrapWithAbbreviation('%s')", szAbbreviation);

    RunInternal(szCmd, EmmetAction_WrapWithAbbreviation);

    HeapFree(GetProcessHeap(), 0, szCmd);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::ToggleComment()
{
    RunInternal("actionToggleComment()", EmmetAction_ToggleComment);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::RemoveTag()
{
    RunInternal("actionRemoveTag()", EmmetAction_RemoveTag);

    return EmmetResult_OK;
}

EmmetResult CEmmetEngine::MergeLines()
{
    return RunInternal("actionMergeLines()", EmmetAction_MergeLines);
}

EmmetResult CEmmetEngine::UpdateImageSize()
{
    return RunInternal("actionUpdateImageSize()", EmmetAction_UpdateImageSize);
}

EmmetResult CEmmetEngine::RunInternal(const char* action, EmmetAction actionCode)
{
    CComPtr<Document> pActiveDoc;
    CComPtr<IDispatch> pDisp;
    CComPtr<TextDocument> pTextDoc;
    CComPtr<TextSelection> pSelection;

    m_pDTE->get_ActiveDocument(&pActiveDoc);
    if (NULL == pActiveDoc)
        return EmmetResult_NoActiveDocument;
    pActiveDoc->Object(CComBSTR("TextDocument"), &pDisp);
    pDisp->QueryInterface(__uuidof(TextDocument), (LPVOID*)&pTextDoc);
    pTextDoc->get_Selection(&pSelection);

    if (!m_pEditorProxy->Initialize(pActiveDoc, pTextDoc, pSelection, actionCode))
        return EmmetResult_DocumentFormatNotSupported;

    TryCatch try_catch;

    // Create a string containing the JavaScript source code.
    Handle<String> source = String::New(action);

    // Compile the source code.
    Handle<Script> script = Script::Compile(source);
  
    // Run the script to get the result.
    Handle<Value> result = script->Run();

    if (result.IsEmpty())
    {
        ReportException(&try_catch);

        return EmmetResult_OK;
    }

    if (result->IsBoolean() || result->IsBooleanObject())
    {
        bool bResult = result->BooleanValue();
        if (!bResult)
        {
            MessageBox(NULL,
                       L"Emmet engine was not able to execute an action",
                       L"Action execution failed",
                       MB_OK | MB_ICONWARNING);

            return EmmetResult_UnexpectedError;
        }
    }

    return EmmetResult_OK;
}