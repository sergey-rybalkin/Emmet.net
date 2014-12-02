#include "stdafx.h"
#include "EmmetEngine.h"

#define USER_SNIPPETS_PATH L"%APPDATA%\\Emmet\\snippets.js"
#define USER_PREFERENCES_PATH L"%APPDATA%\\Emmet\\preferences.js"

using namespace v8;

CEmmetEngine::CEmmetEngine()
{
}

EmmetResult CEmmetEngine::Initialize(_DTE* pDTE, PCWSTR szEngineScriptPath, PCWSTR szHelpersScriptPath)
{
    m_DTE.Attach(pDTE);
	m_editorProxy.Attach(new CEditorProxy());

	m_isolate = Isolate::GetCurrent();
	HandleScope scope(m_isolate);

	// Global object that will store editor interface implementation
    Handle<ObjectTemplate> global = ObjectTemplate::New();
	Handle<ObjectTemplate> editor = m_editorProxy->GetEditorInterfaceImplementation();
	global->Set(String::New("editorProxy"), editor);

	// Context::New returns a persistent handle which is what we need for the reference to remain after we
	// return from this method. That persistent handle has to be disposed in the destructor.
    Handle<Context> context = Context::New(m_isolate, NULL, global);
	m_context.Reset(m_isolate, context);

	// Enter the new context so all the following operations take place within it.
	Context::Scope context_scope(context);

	EmmetResult result = ReadAndCompileScript(szEngineScriptPath);
    if (EmmetResult_OK != result)
        return result;

    result = LoadUserProfile();
    if (EmmetResult_OK != result)
        return result;

    result = ReadAndCompileScript(szHelpersScriptPath);
	if (EmmetResult_OK != result)
        return result;
	
	Handle<String> actionName = String::New("actionExpandAbbreviation");
	Handle<Value> func = context->Global()->Get(actionName);
	if (!func->IsFunction())
		return EmmetResult_UnexpectedError;
	Handle<Function> funcHandle = Handle<Function>::Cast(func);
	m_expandAbbreviationFunc.Reset(m_isolate, funcHandle);

	actionName = String::New("actionWrapWithAbbreviation");
	func = context->Global()->Get(actionName);
	if (!func->IsFunction())
		return EmmetResult_UnexpectedError;
	funcHandle = Handle<Function>::Cast(func);
	m_wrapWithAbbreviationFunc.Reset(m_isolate, funcHandle);

	actionName = String::New("actionToggleComment");
	func = context->Global()->Get(actionName);
	if (!func->IsFunction())
		return EmmetResult_UnexpectedError;
	funcHandle = Handle<Function>::Cast(func);
	m_toggleCommentFunc.Reset(m_isolate, funcHandle);

	actionName = String::New("actionMergeLines");
	func = context->Global()->Get(actionName);
	if (!func->IsFunction())
		return EmmetResult_UnexpectedError;
	funcHandle = Handle<Function>::Cast(func);
	m_mergeLinesFunc.Reset(m_isolate, funcHandle);

	return EmmetResult_OK;
}

CEmmetEngine::~CEmmetEngine(void)
{
    m_context.Dispose();
}

EmmetResult CEmmetEngine::ExpandAbbreviation()
{
    return RunAction(&m_expandAbbreviationFunc, EmmetAction_ExpandAbbreviation);
}

EmmetResult CEmmetEngine::WrapWithAbbreviation(const char* szAbbreviation, UINT nchAbbreviation)
{
    return RunAction(&m_wrapWithAbbreviationFunc, EmmetAction_WrapWithAbbreviation, szAbbreviation);
}

EmmetResult CEmmetEngine::ToggleComment()
{
    return RunAction(&m_toggleCommentFunc, EmmetAction_ToggleComment);
}

EmmetResult CEmmetEngine::MergeLines()
{
    return RunAction(&m_mergeLinesFunc, EmmetAction_MergeLines);
}

EmmetResult CEmmetEngine::RunAction(Persistent<Function>* func,
	                                EmmetAction actionCode,
									const char* param)
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

	HandleScope handleScope(m_isolate);
	Local<Context> context = Local<Context>::New(m_isolate, m_context);
	Context::Scope contextScope(context);
    
	TryCatch try_catch;
	Local<Function> action = Local<Function>::New(m_isolate, *func);


    // All content manipulations will be run in a single undo context so that they can be reverted all at once
    CComPtr<UndoContext> undoContext;
    m_DTE->get_UndoContext(&undoContext);
    undoContext->Open(CComBSTR("Emmet"));

    // Run the script wrapped with a single undo context.
	Handle<Value> result;
	if (param == NULL)
		result = action->Call(context->Global(), 0, NULL);
	else
	{
		Handle<String> arg = String::New(param);
		Handle<Value> argv[1] = { arg };
		result = action->Call(context->Global(), 1, argv);
	}
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

EmmetResult CEmmetEngine::ReadAndCompileScript(PCWSTR szEngineScriptPath)
{
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

    return ExecuteScriptFile(scriptFile);
}

EmmetResult CEmmetEngine::LoadUserProfile()
{
    EmmetResult result = EmmetResult_OK;
    DWORD dwRequiredBuf = ExpandEnvironmentStrings(USER_SNIPPETS_PATH, NULL, 0);
    CAutoPtr<WCHAR> snippetsFilePath(new WCHAR[dwRequiredBuf]);
    ExpandEnvironmentStrings(USER_SNIPPETS_PATH, snippetsFilePath, dwRequiredBuf);
    CAtlFile snippetsFile;
    HRESULT hr = snippetsFile.Create(snippetsFilePath,
                                     GENERIC_READ,
                                     FILE_SHARE_READ,
                                     OPEN_EXISTING,
                                     FILE_FLAG_SEQUENTIAL_SCAN);
    if (SUCCEEDED(hr))
    {
        result = ExecuteScriptFile(snippetsFile);
        if (EmmetResult_OK != result)
            return result;
    }

    dwRequiredBuf = ExpandEnvironmentStrings(USER_PREFERENCES_PATH, NULL, 0);
    CAutoPtr<WCHAR> preferencesFilePath(new WCHAR[dwRequiredBuf]);
    ExpandEnvironmentStrings(USER_PREFERENCES_PATH, preferencesFilePath, dwRequiredBuf);
    
    CAtlFile preferencesFile;
    hr = preferencesFile.Create(preferencesFilePath,
                                GENERIC_READ,
                                FILE_SHARE_READ,
                                OPEN_EXISTING,
                                FILE_FLAG_SEQUENTIAL_SCAN);
    if (SUCCEEDED(hr))
        result = ExecuteScriptFile(preferencesFile);

    return result;
}

EmmetResult CEmmetEngine::ExecuteScriptFile(CAtlFile scriptFile)
{
    ULONGLONG len;
    scriptFile.GetSize(len);
    CAutoPtr<char> fileContent(new char[(DWORD)len + 1]);
    scriptFile.Read(fileContent.m_p, (int)len);
    fileContent.m_p[len] = '\0';

    // Enter the new context so all the following operations take place within it.
    HandleScope scope(m_isolate);
    TryCatch try_catch;
    Handle<Script> script = Script::Compile(String::New(fileContent));

    if (script.IsEmpty())
    {
        FormatExceptionMessage(&try_catch);
        return EmmetResult_UnexpectedError;
    }

    Handle<Value> result = script->Run();
    if (result.IsEmpty())
    {
        FormatExceptionMessage(&try_catch);
        return EmmetResult_UnexpectedError;
    }

    return EmmetResult_OK;
}

VOID CEmmetEngine::FormatExceptionMessage(TryCatch* exceptionInfo)
{
	HandleScope handle_scope(m_isolate);

    m_lastError.Append(L"Emmet JavaScript error: \n");
    
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