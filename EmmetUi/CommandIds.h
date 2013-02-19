// CommandIds.h
// Command IDs used in defining command bars
//

// do not use #pragma once - used by ctc compiler
#ifndef __COMMANDIDS_H_
#define __COMMANDIDS_H_

///////////////////////////////////////////////////////////////////////////////
// Menu IDs

#define EmmetSubMenu 0x1100

///////////////////////////////////////////////////////////////////////////////
// Menu Group IDs

#define EmmetSubMenuGroup           0x1020
#define EmmetTopMenuGroup           0x1030

///////////////////////////////////////////////////////////////////////////////
// Command IDs

#define cmdidExpandAbbreviation   0x100
#define cmdidWrapWithAbbreviation 0x101
#define cmdidToggleComment        0x102
#define cmdidRemoveTag            0x103
#define cmdidMergeLines           0x104
#define cmdidUpdateImageSize      0x105

#define cmdidExpandAbbreviationInternal   0x110
#define cmdidWrapWithAbbreviationInternal 0x111
#define cmdidRemoveTagInternal 0x113


///////////////////////////////////////////////////////////////////////////////
// Bitmap IDs
#define bmpPic1 1
#define bmpPic2 2
#define bmpPicSearch 3
#define bmpPicX 4
#define bmpPicArrows 5
#define bmpPicStrikethrough 6

#endif // __COMMANDIDS_H_
