#include "stdafx.h"
#include "FileProxy.h"

using namespace v8;

Handle<Value> FileReadFile(const Arguments& args)
{
    String::AsciiValue path(args[0]);
    WCHAR szPath[MAX_PATH] = {0};
    MultiByteToWideChar(CP_ACP, 0, *path, path.length(), szPath, MAX_PATH);

    HANDLE hFile = CreateFile(szPath,
                              GENERIC_READ,
                              FILE_SHARE_READ,
                              NULL,
                              OPEN_EXISTING,
                              FILE_FLAG_SEQUENTIAL_SCAN,
                              NULL);

    if (INVALID_HANDLE_VALUE == hFile)
    {
        CString msg(L"Specified file not found: ");
        msg.Append(szPath);
        return ThrowException(String::New(msg));
    }

    char buf[200]; // Full content is not required, first 200 bytes should be enough
    DWORD dwBytesRead;
    ReadFile(hFile, buf, 200, &dwBytesRead, NULL);
    CloseHandle(hFile);

    Handle<Array> retVal = Array::New();

    for (DWORD i=0 ; i<dwBytesRead ; i++)
    {
        retVal->Set(i, Int32::New((unsigned char)((unsigned char*)buf)[i]));
    }

    return retVal;
}

Handle<Value> FileLocateFile(const Arguments& args)
{
    String::AsciiValue editorFilePath(args[0]);
    String::AsciiValue imageSrc(args[1]);

    if (StrStrA(*imageSrc, ":") || imageSrc.length() < 3) // check for non-local and broken image paths
        return ThrowException(String::New("Image URL not supported"));

    int numUpFolders = 0;
    char* src = *imageSrc;
    while (src[0] == '.' && src[1] == '.' && (src[2] == '/' || src[2] == '\\'))
    {
        src += 3;
        numUpFolders++;
    }

    char retVal[MAX_PATH] = {0};
    for (int index = 0; index < editorFilePath.length(); index++)
        retVal[index] = (*editorFilePath)[index];

    int curPos = editorFilePath.length() - 1;

    while (0 != numUpFolders)
    {
        while (curPos > 0 && retVal[--curPos] != '\\');
        numUpFolders--;
    }

    if (retVal[curPos] != '\\')
        retVal[curPos++] = '\\';

    if (*src == '/' || *src == '\\')
        src++;

    while ((src - *imageSrc) < imageSrc.length())
    {
        char ch = *src++;
        retVal[++curPos] = (ch == '/') ? '\\' : ch;
    }
    retVal[curPos + 1] = '\0';

    return String::New(retVal);
}

Handle<Value> FileCreatePath(const Arguments& args)
{
    return ThrowException(String::New("CreatePath not implemented"));
}

Handle<Value> FileSaveFile(const Arguments& args)
{
    return ThrowException(String::New("SaveFile not implemented"));
}

CFileProxy::CFileProxy()
{
}
CFileProxy::~CFileProxy()
{
}

VOID CFileProxy::Register(Handle<ObjectTemplate> global)
{
    global->Set(String::New("File_ReadFile"), FunctionTemplate::New(FileReadFile));
    global->Set(String::New("File_LocateFile"), FunctionTemplate::New(FileLocateFile));
    global->Set(String::New("File_CreatePath"), FunctionTemplate::New(FileCreatePath));
    global->Set(String::New("File_SaveFile"), FunctionTemplate::New(FileSaveFile));
}