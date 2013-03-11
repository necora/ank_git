// $Id: IStatusImageMapper.cs 5699 2008-12-12 13:03:53Z rhuijben $
//
// Copyright 2008 The AnkhSVN Project
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
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Ankh.Scc
{
    public interface IStatusImageMapper
    {
        AnkhGlyph GetStatusImageForSvnItem(SvnItem item);
        ImageList StatusImageList { get; }

        ImageList CreateStatusImageList();
    }
}