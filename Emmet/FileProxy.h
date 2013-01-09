#pragma once

using namespace v8;

class CFileProxy
{
public:
    CFileProxy(void);
    ~CFileProxy(void);

    VOID Register(Handle<ObjectTemplate> global);
};