#pragma once

#include "EditorCallbacks.h"

using namespace v8;

class CEditorProxy
{
public:
    CEditorProxy(void);
    ~CEditorProxy(void);

	Handle<ObjectTemplate> GetEditorInterfaceImplementation();

    BOOL Initialize(Document* pDoc, TextDocument* pTextDoc, TextSelection* pSelection, EmmetAction action); 
};