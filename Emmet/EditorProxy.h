#pragma once

using namespace v8;

enum EmmetAction
{
    EmmetAction_ExpandAbbreviation,

    EmmetAction_WrapWithAbbreviation,

    EmmetAction_ToggleComment,

    EmmetAction_RemoveTag,

    EmmetAction_MergeLines,

    EmmetAction_UpdateImageSize
};

class CEditorProxy
{
public:
    CEditorProxy(void);
    ~CEditorProxy(void);

    VOID Register(Handle<ObjectTemplate> global);

    VOID SetContext(Persistent<v8::Context> context);

    BOOL Initialize(Document* pDoc, TextDocument* pTextDoc, TextSelection* pSelection, EmmetAction action);
};