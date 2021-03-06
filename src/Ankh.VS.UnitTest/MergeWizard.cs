// $Id: MergeWizard.cs 7184 2009-08-12 13:31:02Z rhuijben $
//
// Copyright 2009 The AnkhSVN Project
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
using NUnit.Framework;
using SharpSvn;
using Ankh.UI.MergeWizard;
using NUnit.Framework.SyntaxHelpers;

namespace Ankh.Tests
{
    [TestFixture]
    public class MergeWizardTest
    {
        [Test]
        public void TestMergeRangeToString()
        {
            List<SvnRevisionRange> revisions = new List<SvnRevisionRange>();

            Assert.That(
                MergeWizard.MergeRevisionsAsString(revisions),
                Is.EqualTo(""));

            revisions.Add(new SvnRevisionRange(2, 3));

            Assert.That(
                MergeWizard.MergeRevisionsAsString(revisions),
                Is.EqualTo("3"));

            revisions.Add(new SvnRevisionRange(6, 7));
            
            Assert.That(
                MergeWizard.MergeRevisionsAsString(revisions),
                Is.EqualTo("3, 7"));

            revisions.Add(new SvnRevisionRange(10, 15));

            Assert.That(
                MergeWizard.MergeRevisionsAsString(revisions),
                Is.EqualTo("3, 7, 11-15"));
        }
    }
}
