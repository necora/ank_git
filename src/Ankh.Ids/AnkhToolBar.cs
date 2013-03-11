// $Id: AnkhToolBar.cs 8442 2010-05-12 00:14:50Z rhuijben $
//
// Copyright 2008-2009 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Runtime.InteropServices;

namespace Ankh
{
    // Disable Missing XML comment warning for this file
#pragma warning disable 1591 

    [Guid(AnkhId.CommandSet)]
    public enum AnkhToolBar
    {
        None=0,
        ToolBarFirst = 0x7FFFFFF,
        PendingChanges,
        SourceControl,
        LogViewer,
        SvnInfo
    }
}