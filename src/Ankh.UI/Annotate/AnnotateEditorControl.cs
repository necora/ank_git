// $Id: AnnotateEditorControl.cs 8368 2010-05-05 13:49:44Z rhuijben $
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using SharpSvn;

using Ankh.Commands;
using Ankh.Scc;
using Ankh.Scc.UI;
using Ankh.UI.PendingChanges;
using Ankh.UI.VSSelectionControls;
using System.Drawing;

namespace Ankh.UI.Annotate
{
    public partial class AnnotateEditorControl : VSEditorControl, ISelectionMapOwner<IAnnotateSection>
    {
        List<AnnotateRegion> blameSections = new List<AnnotateRegion>();
        SelectionItemMap _map;
        SvnOrigin _origin;

        public AnnotateEditorControl()
        {
            InitializeComponent();
            editor.ReadOnly = true;

            if (VSVersion.VS2008OrOlder)
                editor.EnableNavigationBar = true;
        }

        protected override void OnFrameCreated(EventArgs e)
        {
            base.OnFrameCreated(e);

            if (DesignMode)
                return;

            _map = SelectionItemMap.Create<IAnnotateSection>(this);
            _map.Context = Context;

            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
            // Set Notify that we have a selection, otherwise the first selection request fails.
            _map.NotifySelectionUpdated();

            CommandContext = AnkhId.AnnotateContextGuid;
            KeyboardContext = new Guid(0x8B382828, 0x6202, 0x11d1, 0x88, 0x70, 0x00, 0x00, 0xF8, 0x75, 0x79, 0xD2); // Editor
            SetFindTarget(editor.FindTarget);

            blameMarginControl1.Init(Context, this, blameSections);
        }

        public void LoadFile(string annotateFile)
        {
            // Does this anything?
            this.Text = Path.GetFileName(annotateFile) + " (Annotated)";

            editor.LoadFile(annotateFile, false);
        }

        public void SetLanguageService(Guid language)
        {
            editor.ForceLanguageService = language;
        }

        internal int GetLineHeight()
        {
            return editor.LineHeight;
        }

        internal int GetTopLine()
        {
            // TODO: implement real fix for VS2010
            if (VSVersion.VS2010)
                return 0;

            Point p = editor.EditorClientTopLeft;
            return PointToClient(p).Y;
        }

        public void AddLines(SvnOrigin origin, Collection<SharpSvn.SvnBlameEventArgs> blameResult, Dictionary<long, string> logMessages)
        {
            _origin = origin;

            SortedList<long, AnnotateSource> _sources = new SortedList<long, AnnotateSource>();

            AnnotateRegion section = null;
            blameSections.Clear();

            long min = -1;
            long max = -1;
            foreach (SvnBlameEventArgs e in blameResult)
            {
                AnnotateSource src;
                if (!_sources.TryGetValue(e.Revision, out src))
                {
                    _sources.Add(e.Revision, src = new AnnotateSource(e, origin));

                    string msg;
                    if (logMessages.TryGetValue(e.Revision, out msg))
                        src.LogMessage = msg ?? "";
                    else
                    {
                        if (min >= 0 && min < e.Revision)
                            min = e.Revision;

                        if (e.Revision > max)
                            max = e.Revision;
                    }
                }

                int line = (int)e.LineNumber;

                if (section == null || section.Source != src)
                {
                    section = new AnnotateRegion(line, src);
                    blameSections.Add(section);
                }
                else
                {
                    section.EndLine = line;
                }
            }
            blameMarginControl1.Invalidate();

            RetrieveLogs(origin, _sources, min, max);
        }

        private void RetrieveLogs(SvnOrigin origin, SortedList<long, AnnotateSource> _sources, long min, long max)
        {
            if (_sources.Count == 0 || min == -1 || max == -1)
                return;

            EventHandler<SvnLogEventArgs> lr =
                delegate(object sender, SvnLogEventArgs e)
                {
                    AnnotateSource src;
                    if (_sources.TryGetValue(e.Revision, out src))
                        src.LogMessage = e.LogMessage ?? "";
                };

            AnkhAction aa = delegate()
            {
                using (SvnClient cl = Context.GetService<ISvnClientPool>().GetClient())
                {
                    SvnLogArgs la = new SvnLogArgs();
                    la.OperationalRevision = origin.Target.Revision;
                    la.Start = max;
                    la.End = min;
                    la.ThrowOnError = false;

                    cl.Log(origin.Uri, la, lr);
                }
            };

            aa.BeginInvoke(null, null); // Start retrieving logs
        }

        private void logMessageEditor1_VerticalScroll(object sender, VSTextEditorScrollEventArgs e)
        {
            blameMarginControl1.NotifyVerticalScroll(e);
        }

        AnnotateSource _selected;
        internal AnnotateSource Selected
        {
            get { return _selected; }
        }

        internal void SetSelection(IAnnotateSection section)
        {
            // Check if necessary
            //Focus();
            //Select();

            _selected = (AnnotateSource)section;

            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);

            _map.NotifySelectionUpdated();
        }

        bool _disabledOutlining;
        protected override void OnFrameLoaded(EventArgs e)
        {
            base.OnFrameLoaded(e);

            editor.Select(); // This should enable the first OnIdle to disable the outlining
            DisableOutliningOnIdle();
        }

        void DisableOutliningOnIdle()
        {
            Context.GetService<IAnkhCommandService>().PostIdleAction(DisableOutlining);
        }

        void DisableOutlining()
        {
            if (!IsDisposed && !_disabledOutlining)
            {
                Guid vs2kcmds = VSConstants.VSStd2K;
                if (editor.ContainsFocus)
                {
                    int n = editor.EditorCommandTarget.Exec(ref vs2kcmds, (uint)VSConstants.VSStd2KCmdID.OUTLN_STOP_HIDING_ALL, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);

                    if (n == 0)
                    {
                        _disabledOutlining = true;
                        return;
                    }
                }

                DisableOutliningOnIdle();
            }
        }

        #region ISelectionMapOwner<IAnnotateSection> Members

        public event EventHandler SelectionChanged;

        System.Collections.IList ISelectionMapOwner<IAnnotateSection>.Selection
        {
            get
            {
                if (_selected == null)
                    return new AnnotateSource[0];

                return new AnnotateSource[] { _selected };
            }
        }

        System.Collections.IList ISelectionMapOwner<IAnnotateSection>.AllItems
        {
            get { return ((ISelectionMapOwner<IAnnotateSection>)this).Selection; }
        }

        IntPtr ISelectionMapOwner<IAnnotateSection>.GetImageList()
        {
            return IntPtr.Zero;
        }

        int ISelectionMapOwner<IAnnotateSection>.GetImageListIndex(IAnnotateSection item)
        {
            return 0;
        }

        string ISelectionMapOwner<IAnnotateSection>.GetText(IAnnotateSection item)
        {
            return item.Revision.ToString();
        }

        public object GetSelectionObject(IAnnotateSection item)
        {
            return item;
        }

        public IAnnotateSection GetItemFromSelectionObject(object item)
        {
            return (IAnnotateSection)item;
        }

        void ISelectionMapOwner<IAnnotateSection>.SetSelection(IAnnotateSection[] items)
        {
            if (items.Length > 0)
                SetSelection(items[0]);
            else
                SetSelection((IAnnotateSection)null);
        }

        /// <summary>
        /// Gets the canonical (path / uri) of the item. Used by packages to determine a selected file
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>A canonical name or null</returns>
        string ISelectionMapOwner<IAnnotateSection>.GetCanonicalName(IAnnotateSection item)
        {
            return null;
        }

        #endregion

        Control ISelectionMapOwner<IAnnotateSection>.Control
        {
            get { return this; }
        }
    }
}
