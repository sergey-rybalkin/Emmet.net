#include "stdafx.h"
#include "EditorCallbacks.h"

CallbackOptions g_Options;

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
	retVal->Set(String::New("start"), Int32::New(start));
	retVal->Set(String::New("end"), Int32::New(end));

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
		Handle<Function>::Cast(context->Global()->Get(String::New("createRange")));

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
	CComBSTR currentLine;

	g_Options.m_selection->get_ActivePoint(&virtualPoint);
	virtualPoint->CreateEditPoint(&editPoint);
	editPoint->StartOfLine();
	editPoint->get_LineLength(&lineLen);
	editPoint->GetText(CComVariant(lineLen), &currentLine);

	CHAR asciiBuf[256] = { 0 };

	int nChars = currentLine.Length();
	WideCharToMultiByte(CP_ACP, 0, currentLine, nChars, asciiBuf, 2048, NULL, NULL);

	args.GetReturnValue().Set(String::New(asciiBuf, nChars));
}

void EditorReplaceContent(const FunctionCallbackInfo<Value>& args)
{
	Local<Value> newContent = args[0];

	CComPtr<VirtualPoint> virtualPoint;

	long start, end = 0;

	g_Options.m_selection->get_ActivePoint(&virtualPoint);
	String::Utf8Value utfContent(newContent);

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
    editPoint->Insert(CComBSTR(utfContent.length(), *utfContent));

	if (EmmetAction_MergeLines != g_Options.m_curAction)
	{
		g_Options.m_selection->MoveToAbsoluteOffset(start + 1, TRUE);
		if (g_Options.m_isHtml)
			g_Options.m_selection->SmartFormat();
	}
}

void EditorGetContent(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<TextPoint> startPoint;
	CComPtr<TextPoint> endPoint;
	CComPtr<EditPoint> startEditPoint;
	CComPtr<EditPoint> endEditPoint;
	CComBSTR buf;

	g_Options.m_textDoc->get_StartPoint(&startPoint);
	g_Options.m_textDoc->get_EndPoint(&endPoint);
	startPoint->CreateEditPoint(&startEditPoint);
	endPoint->CreateEditPoint(&endEditPoint);

	startEditPoint->GetText(CComVariant(endEditPoint), &buf);

	int nChars = buf.Length();
	CAutoPtr<char> asciiBuf(new char[nChars + 1]);

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

    args.GetReturnValue().Set(String::New(asciiBuf, destIndex - 1));
}

void EditorGetSelection(const FunctionCallbackInfo<Value>& args)
{
	CComPtr<VirtualPoint> virtualPoint;
	CComBSTR buf;

	g_Options.m_selection->get_Text(&buf);

	int retValLen = buf.Length();
    if (0 == retValLen)
    {
        args.GetReturnValue().Set(String::New(""));
        return;
    }

	CAutoPtr<char> asciiBuf(new char[retValLen + 1]);

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

    return args.GetReturnValue().Set(retVal);
}

void EditorGetSyntax(const FunctionCallbackInfo<Value>& args)
{
    Handle<String> retVal;
	if (!g_Options.m_isHtml)
        retVal = String::New("css");
	else
        retVal = String::New("html");

    args.GetReturnValue().Set(retVal);
}

void EditorGetProfileName(const FunctionCallbackInfo<Value>& args)
{
    Handle<String> retVal;
    if (!g_Options.m_isHtml)
        retVal = String::New("css");
    else
        retVal = String::New("html");

    args.GetReturnValue().Set(retVal);
}

void EditorPrompt(const FunctionCallbackInfo<Value>& args)
{
	// Default wrap container, probably makes sense to show a dialog box here as well
	return args.GetReturnValue().Set(String::New("div"));
}