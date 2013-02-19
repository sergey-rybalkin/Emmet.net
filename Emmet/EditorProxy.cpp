#include "stdafx.h"
#include "EditorProxy.h"
#include <memory.h>

using namespace v8;

static Persistent<v8::Context> g_Context;

static Document* g_pDoc;
static TextDocument* g_pTextDoc;
static TextSelection* g_pSelection;

static bool g_isHtml = true;

static EmmetAction g_CurAction;

Handle<Value> EditorGetSelectionRange(const Arguments& args)
{
    CComPtr<VirtualPoint> topPoint;
    CComPtr<VirtualPoint> bottomPoint;
    long start;
    long startLineIndex;
    long end;
    long endLineIndex;
    g_pSelection->get_TopPoint(&topPoint);
    topPoint->get_AbsoluteCharOffset(&start);
    g_pSelection->get_BottomPoint(&bottomPoint);
    bottomPoint->get_AbsoluteCharOffset(&end);

    topPoint->get_Line(&startLineIndex);
    bottomPoint->get_Line(&endLineIndex);

    // According to the msdn documentation character numbering begins at one and should be adjusted to our 
    // 0-based numbering.
    start--;
    end--;

    Handle<Object> retVal = Object::New();
    retVal->Set(String::New("start"), Int32::New(start));
    retVal->Set(String::New("end"), Int32::New(end));

    return retVal;
}

Handle<Value> EditorCreateSelection(const Arguments& args)
{
    long start = (long)args[0].As<Integer>()->Value() + 1;
    g_pSelection->MoveToAbsoluteOffset(start);
    
    if (args.Length() == 2 && args[1]->IsNumber())
    {
        long endVal = (long)args[1].As<Integer>()->Value() + 1;
        g_pSelection->MoveToAbsoluteOffset(endVal, TRUE);
    }
    
    return Undefined();
}

Handle<Value> EditorGetCurrentLineRange(const Arguments& args)
{
    CComPtr<VirtualPoint> point;
    long charOffset;
    long lineLen;
    long absCharOffset;
    g_pSelection->get_ActivePoint(&point);
    point->get_AbsoluteCharOffset(&absCharOffset);
    point->get_LineCharOffset(&charOffset);
    point->get_LineLength(&lineLen);
    
    long lineOffset = absCharOffset - charOffset;

    Local<Function> fCreateRange =
        Local<Function>::Cast(g_Context->Global()->Get(String::New("createRange")));

    Handle<Value> argv[] = { Int32::New(lineOffset), Int32::New(lineOffset + lineLen) };

    return fCreateRange->Call(fCreateRange, 2, argv);
}

Handle<Value> EditorGetCaretPos(const Arguments& args)
{
    CComPtr<VirtualPoint> point;
    long retVal;

    g_pSelection->get_ActivePoint(&point);
    point->get_AbsoluteCharOffset(&retVal);

    return Int32::New(retVal - 1);
}

Handle<Value> EditorSetCaretPos(const Arguments& args)
{
    return EditorCreateSelection(args);
}

Handle<Value> EditorGetCurrentLine(const Arguments& args)
{
    CComPtr<VirtualPoint> virtualPoint;
    CComPtr<EditPoint> editPoint;

    long lineLen;
    CComBSTR currentLine;

    g_pSelection->get_ActivePoint(&virtualPoint);
    virtualPoint->CreateEditPoint(&editPoint);
    editPoint->StartOfLine();
    editPoint->get_LineLength(&lineLen);
    editPoint->GetText(CComVariant(lineLen), &currentLine);

    CHAR asciiBuf[256] = {0};

    int nChars = currentLine.Length();
    WideCharToMultiByte(CP_ACP, 0, currentLine, nChars, asciiBuf, 2048, NULL, NULL);

    return String::New(asciiBuf, nChars);
}

Handle<Value> EditorReplaceContent(const Arguments& args)
{
    Local<Value> newContent = args[0];

    CComPtr<VirtualPoint> virtualPoint;

    long start, end = 0;

    g_pSelection->get_ActivePoint(&virtualPoint);
    String::AsciiValue asciiContent(newContent);

    if (args.Length() == 0) // no start/end range
    {
        g_pSelection->SelectAll();
        g_pSelection->Delete();
    }
    else if (args.Length() == 1) // only start point specified
    {
        start = (long)args[1].As<Integer>()->Value();
        g_pSelection->MoveToAbsoluteOffset(start + 1);
    }
    else // both start / end specified
    {
        start = (long)args[1].As<Integer>()->Value();
        end = (long)args[2].As<Integer>()->Value();
        g_pSelection->MoveToAbsoluteOffset(start + 1);
        g_pSelection->MoveToAbsoluteOffset(end + 1, TRUE);
        g_pSelection->Delete();
    }

    CComPtr<VirtualPoint> activePoint;
    CComPtr<EditPoint> editPoint;
    g_pSelection->get_ActivePoint(&activePoint);
    activePoint->CreateEditPoint(&editPoint);
    editPoint->Insert(CComBSTR(asciiContent.length(), *asciiContent));

    if (EmmetAction_ExpandAbbreviation == g_CurAction ||
        EmmetAction_WrapWithAbbreviation == g_CurAction ||
        EmmetAction_RemoveTag == g_CurAction)
    {
        g_pSelection->MoveToAbsoluteOffset(start + 1, TRUE);
        g_pSelection->SmartFormat();
    }

    return Undefined();
}

Handle<Value> EditorGetContent(const Arguments& args)
{
    CComPtr<TextPoint> startPoint;
    CComPtr<TextPoint> endPoint;
    CComPtr<EditPoint> startEditPoint;
    CComPtr<EditPoint> endEditPoint;
    CComBSTR buf;

    g_pTextDoc->get_StartPoint(&startPoint);
    g_pTextDoc->get_EndPoint(&endPoint);
    startPoint->CreateEditPoint(&startEditPoint);
    endPoint->CreateEditPoint(&endEditPoint);

    startEditPoint->GetText(CComVariant(endEditPoint), &buf);

    int nChars = buf.Length();
    PSTR asciiBuf = (PSTR)HeapAlloc(GetProcessHeap(), 0, nChars + 1);

    WideCharToMultiByte(CP_ACP, 0, buf, nChars + 1, asciiBuf, nChars + 1, NULL, NULL);

    int destIndex = 0;
    for (int index = 0; index <= nChars; index++)
    {
        if (asciiBuf[index] != '\r')
        {
            asciiBuf[destIndex] = asciiBuf[index];
            destIndex++;
        }
    }
    if (destIndex < nChars)
        asciiBuf[destIndex] = '\0';

    Handle<String> retVal = String::New(asciiBuf, destIndex - 1);

    HeapFree(GetProcessHeap(), 0, asciiBuf);

    return retVal;
}

Handle<Value> EditorGetSelection(const Arguments& args)
{
    CComPtr<VirtualPoint> virtualPoint;
    CComBSTR buf;    

    g_pSelection->get_Text(&buf);

    int retValLen = buf.Length();
    if (0 == retValLen)
        return String::New("");

    PSTR asciiBuf = (PSTR)HeapAlloc(GetProcessHeap(), 0, retValLen + 1);

    WideCharToMultiByte(CP_ACP, 0, buf, retValLen + 1, asciiBuf, retValLen + 1, NULL, NULL);
    asciiBuf[retValLen] = '\0';

    int destIndex = 0;
    for (int index = 0; index <= retValLen; index++)
    {
        if (asciiBuf[index] != '\r')
        {
            asciiBuf[destIndex] = asciiBuf[index];
            destIndex++;
        }
    }
    asciiBuf[destIndex] = '\0';

    Handle<String> retVal = String::New(asciiBuf, destIndex);

    HeapFree(GetProcessHeap(), 0, asciiBuf);

    return retVal;
}

Handle<Value> EditorGetSyntax(const Arguments& args)
{
    if (!g_isHtml)
        return String::New("css");
    else
        return String::New("html");
}

Handle<Value> EditorGetProfileName(const Arguments& args)
{
    if (!g_isHtml)
        return String::New("css");
    else
        return String::New("html");
}

Handle<Value> EditorPrompt(const Arguments& args)
{
    // Default wrap container, probably makes sense to show a dialog box here as well
    return String::New("div");
}

Handle<Value> EditorGetFilePath(const Arguments& args)
{
    CComBSTR filePath;
    g_pDoc->get_Path(&filePath);

    return String::New(filePath);
}

CEditorProxy::CEditorProxy(void)
{
    g_pDoc = NULL;
}

CEditorProxy::~CEditorProxy(void)
{
}

VOID CEditorProxy::Register(Handle<ObjectTemplate> global)
{
    Handle<ObjectTemplate> editorProxy = ObjectTemplate::New();
    global->Set(String::New("editorProxy"), editorProxy);

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
    editorProxy->Set(String::New("getFilePath"), FunctionTemplate::New(EditorGetFilePath));
}

VOID CEditorProxy::SetContext(Persistent<v8::Context> context)
{
    g_Context = context;
}

BOOL CEditorProxy::Initialize(Document* pDoc, TextDocument* pTextDoc, TextSelection* pSelection, EmmetAction action)
{
    g_pDoc = pDoc;
    g_pTextDoc = pTextDoc;
    g_pSelection = pSelection;
    g_CurAction = action;

    CComBSTR bstrLang;
    CComBSTR bstrName;
    pDoc->get_Language(&bstrLang);
    pDoc->get_Name(&bstrName);

    // We need to distinguish HTML and CSS syntaxes only, it doesn't really matter whether CSS is actually
    // LESS or SASS document
    g_isHtml = bstrLang == "HTML";

    return TRUE;
}