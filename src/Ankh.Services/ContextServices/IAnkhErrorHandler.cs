// $Id: IAnkhErrorHandler.cs 6373 2009-03-10 23:23:39Z rhuijben $
//
// Copyright 2004-2009 The AnkhSVN Project
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
using Ankh.Commands;

namespace Ankh
{
    /// <summary>
    /// Represents a class that handles an Ankh error.
    /// </summary>
    public interface IAnkhErrorHandler
    {
        bool IsEnabled(Exception ex);

        void OnError(Exception ex);

        void OnError(Exception ex, BaseCommandEventArgs commandInfo);
    }
}