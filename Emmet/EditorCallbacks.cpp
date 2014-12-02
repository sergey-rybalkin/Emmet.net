#include "stdafx.h"
#include "EditorCallbacks.h"

CallbackOptions g_Options;

// Visual Studio API returns offsets without considering \r symbol, however Emmet engine does consider it. In 
// order to unify offsets generation we'll have to remove all \r symbols using this function from strings that
// we are returning to the Emmet engine.
void StripCarriageReturns(BSTR str, BSTR* pOut)
{
    CComBSTR source;
    source.Attach(str);
    int len = source.Length();
    CAutoPtr<WCHAR> destination(new WCHAR[len + 1]);

    int destIndex = 0;
    for (int srcIndex = 0; srcIndex < len; srcIndex++)
    {
        WCHAR ch = source[srcIndex];
        if (ch != L'\r')
            destination[destIndex++] = ch;
    }
    destination[destIndex] = 0;

    *pOut = SysAllocString(destination);
}

void EditorUpdateExecutionOptions(CallbackOptions options)
{
	g_Options = options;
}

void EditorGetSelectionRange(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<VirtualPoint> topPoint;
	CComPtr<VirtualPoint> bottomPoint;
	long start;
	long startLineIndex;
	long end;
	long endLineIndex;
	g_Options.m_selection->get_TopPoint(&topPoint);
	topPoint->get_AbsoluteCharOffset(&start);
	g_Options.m_selection->get_BottomPoint(&bottomPoint);
	bottomPoint->get_AbsoluteCharOffset(&end);

	topPoint->get_Line(&startLineIndex);
	bottomPoint->get_Line(&endLineIndex);

	// According to the msdn documentation character numbering begins at one and should be adjusted to our 
	// 0-based numbering.
	start--;
	end--;

	Handle<Object> retVal = Object::New();
	retVal->Set(String::New(L"start"), Int32::New(start));
	retVal->Set(String::New(L"end"), Int32::New(end));

	args.GetReturnValue().Set(retVal);
}

void EditorCreateSelection(const FunctionCallbackInfo<Value>& args)
{
	long start = (long)args[0].As<Integer>()->Value() + 1;
	g_Options.m_selection->MoveToAbsoluteOffset(start);

	if (args.Length() == 2 && args[1]->IsNumber())
	{
		long endVal = (long)args[1].As<Integer>()->Value() + 1;
		g_Options.m_selection->MoveToAbsoluteOffset(endVal, TRUE);
	}
}

void EditorGetCurrentLineRange(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<VirtualPoint> point;
	long charOffset;
	long lineLen;
	long absCharOffset;
	g_Options.m_selection->get_ActivePoint(&point);
	point->get_AbsoluteCharOffset(&absCharOffset);
	point->get_LineCharOffset(&charOffset);
	point->get_LineLength(&lineLen);

	long lineOffset = absCharOffset - charOffset;

	Local<Context> context = Isolate::GetCurrent()->GetCurrentContext();
	Handle<Function> fCreateRange =
		Handle<Function>::Cast(context->Global()->Get(String::New(L"createRange")));

	Handle<Value> argv[] = { Int32::New(lineOffset), Int32::New(lineOffset + lineLen) };

	Handle<Value> retVal = fCreateRange->Call(fCreateRange, 2, argv);

	args.GetReturnValue().Set(retVal);
}

void EditorGetCaretPos(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<VirtualPoint> point;
	long retVal;

	g_Options.m_selection->get_ActivePoint(&point);
	point->get_AbsoluteCharOffset(&retVal);

	args.GetReturnValue().Set(retVal - 1);
}

void EditorSetCaretPos(const FunctionCallbackInfo<Value>& args)
{
	EditorCreateSelection(args);
}

void EditorGetCurrentLine(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<VirtualPoint> virtualPoint;
	CComPtr<EditPoint> editPoint;

	long lineLen;
	BSTR currentLine;
    BSTR retVal;

	g_Options.m_selection->get_ActivePoint(&virtualPoint);
	virtualPoint->CreateEditPoint(&editPoint);
	editPoint->StartOfLine();
	editPoint->get_LineLength(&lineLen);
	editPoint->GetText(CComVariant(lineLen), &currentLine);

    StripCarriageReturns(currentLine, &retVal);

    args.GetReturnValue().Set(String::New(retVal, SysStringLen(retVal)));

    SysFreeString(retVal);
}

void EditorReplaceContent(const FunctionCallbackInfo<Value>& args)
{
	Local<Value> newContent = args[0];

	CComPtr<VirtualPoint> virtualPoint;

	long start, end = 0;

	g_Options.m_selection->get_ActivePoint(&virtualPoint);

	String::Utf8Value utf8Content(newContent);

    // As V8 always returns utf8 string we need to convert it to utf16 before it can be used.
    int contentLen = MultiByteToWideChar(CP_UTF8, NULL, *utf8Content, -1, NULL, 0);
    CAutoPtr<WCHAR> content(new WCHAR[contentLen + 1]);
    MultiByteToWideChar(CP_UTF8, NULL, *utf8Content, -1, content, contentLen + 1);

	if (args.Length() == 0) // no start/end range
	{
		g_Options.m_selection->SelectAll();
		g_Options.m_selection->Delete();
	}
	else if (args.Length() == 1) // only start point specified
	{
		start = (long)args[1].As<Integer>()->Value();
		g_Options.m_selection->MoveToAbsoluteOffset(start + 1);
	}
	else // both start / end specified
	{
		start = (long)args[1].As<Integer>()->Value();
		end = (long)args[2].As<Integer>()->Value();
		g_Options.m_selection->MoveToAbsoluteOffset(start + 1);
		g_Options.m_selection->MoveToAbsoluteOffset(end + 1, TRUE);
		g_Options.m_selection->Delete();
	}

	CComPtr<VirtualPoint> activePoint;
	CComPtr<EditPoint> editPoint;
	g_Options.m_selection->get_ActivePoint(&activePoint);
	activePoint->CreateEditPoint(&editPoint);

    BSTR bstrContent = SysAllocString(content);
    editPoint->Insert(bstrContent);
    SysFreeString(bstrContent);

	if (EmmetAction_MergeLines != g_Options.m_curAction)
	{
		g_Options.m_selection->MoveToAbsoluteOffset(start + 1, TRUE);
		if (g_Options.m_syntax == EmmetSyntax_Html)
			g_Options.m_selection->SmartFormat();
	}
}

void EditorGetContent(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<TextPoint> startPoint;
	CComPtr<TextPoint> endPoint;
	CComPtr<EditPoint> startEditPoint;
	CComPtr<EditPoint> endEditPoint;
	BSTR buf;
    BSTR retVal;

	g_Options.m_textDoc->get_StartPoint(&startPoint);
	g_Options.m_textDoc->get_EndPoint(&endPoint);
	startPoint->CreateEditPoint(&startEditPoint);
	endPoint->CreateEditPoint(&endEditPoint);

	startEditPoint->GetText(CComVariant(endEditPoint), &buf);

    StripCarriageReturns(buf, &retVal);

	args.GetReturnValue().Set(String::New(retVal, SysStringLen(retVal)));

    SysFreeString(retVal);
}

void EditorGetSelection(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<VirtualPoint> virtualPoint;
    BSTR buf;
    BSTR retVal;

	g_Options.m_selection->get_Text(&buf);

	int retValLen = SysStringLen(buf);
    if (0 == retValLen)
    {
        args.GetReturnValue().Set(String::New(L""));
        SysFreeString(buf);
        return;
    }

    StripCarriageReturns(buf, &retVal);

    args.GetReturnValue().Set(String::New(retVal, SysStringLen(retVal)));

    SysFreeString(retVal);
}

void EditorGetSyntax(const FunctionCallbackInfo<Value>& args)
{
    Handle<String> retVal;
    if (g_Options.m_syntax == EmmetSyntax_Html)
        retVal = String::New(L"html");
    else if (g_Options.m_syntax == EmmetSyntax_Scss)
        retVal = String::New(L"scss");
    else
        retVal = String::New(L"css");
    
    args.GetReturnValue().Set(retVal);
}

void EditorGetProfileName(const FunctionCallbackInfo<Value>& args)
{
    Handle<String> retVal;
    if (g_Options.m_syntax == EmmetSyntax_Html)
        retVal = String::New(L"html");
    else
        retVal = String::New(L"css");

    args.GetReturnValue().Set(retVal);
}

void EditorPrompt(const FunctionCallbackInfo<Value>& args)
{
	// Default wrap container, probably makes sense to show a dialog box here as well
	return args.GetReturnValue().Set(String::New(L"div"));
}