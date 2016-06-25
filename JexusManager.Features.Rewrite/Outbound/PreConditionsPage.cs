﻿// Copyright (c) Lex Li. All rights reserved.
// 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InboundRulePage.cs" company="LeXtudio">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace JexusManager.Features.Rewrite.Outbound
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Forms;

    using JexusManager.Properties;
    using JexusManager.Services;

    using Microsoft.Web.Management.Client;
    using Microsoft.Web.Management.Client.Win32;

    internal partial class PreConditionsPage : ModuleListPage, IModuleChildPage
    {
        private sealed class PageTaskList : DefaultTaskList
        {
            private readonly PreConditionsPage _owner;

            public PageTaskList(PreConditionsPage owner)
            {
                _owner = owner;
            }

            public override ICollection GetTaskItems()
            {
                return new TaskItem[]
                           {
                               GetBackTaskItem("Back", "Back to Rules"),
                               new MethodTaskItem(string.Empty, "-", string.Empty).SetUsage(),
                               HelpTaskItem
                           };
            }

            [Obfuscation(Exclude = true)]
            public void ShowHelp()
            {
                _owner.ShowHelp();
            }

            [Obfuscation(Exclude = true)]
            public void Back()
            {
                _owner.Back();
            }
        }

        private sealed class PreConditionListViewItem : ListViewItem
        {
            public PreConditionItem Item { get; }
            private readonly PreConditionsPage _page;

            public PreConditionListViewItem(PreConditionItem item, PreConditionsPage page)
                : base(item.Name)
            {
                this.Item = item;
                _page = page;
                this.SubItems.Add(new ListViewSubItem(this, item.Conditions.Count.ToString()));
                this.SubItems.Add(new ListViewSubItem(this, item.Flag));
            }

            public void Update()
            {
                // TODO: how to update.
            }
        }

        private TaskList _taskList;
        private PreConditionsFeature _feature;

        public PreConditionsPage()
        {
            this.InitializeComponent();
        }

        protected override void Initialize(object navigationData)
        {
            base.Initialize(navigationData);
            var service = (IConfigurationService)GetService(typeof(IConfigurationService));
            pictureBox1.Image = service.Scope.GetImage();

            _feature = new PreConditionsFeature(Module);
            _feature.RewriteSettingsUpdated = this.InitializeListPage;
            _feature.Load();
        }

        protected override void InitializeListPage()
        {
            listView1.Items.Clear();
            foreach (var file in _feature.Items)
            {
                listView1.Items.Add(new PreConditionListViewItem(file, this));
            }

            if (_feature.SelectedItem == null)
            {
                this.Refresh();
                return;
            }

            foreach (PreConditionListViewItem item in listView1.Items)
            {
                if (item.Item == _feature.SelectedItem)
                {
                    item.Selected = true;
                }
            }
        }

        private void ListView1SelectedIndexChanged(object sender, EventArgs e)
        {
            _feature.SelectedItem = listView1.SelectedItems.Count > 0
                ? ((PreConditionListViewItem)listView1.SelectedItems[0]).Item
                : null;
            this.Refresh();
        }

        private void ListView1KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                _feature.Remove();
            }
        }

        protected override void Refresh()
        {
            this.Tasks.Fill(tsActionPanel, cmsActionPanel);
            base.Refresh();
        }

        protected override bool ShowHelp()
        {
            Process.Start("http://go.microsoft.com/fwlink/?LinkID=130425&amp;clcid=0x409");
            return true;
        }

        private void SplitContainer1SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (splitContainer1.Panel2.Width > 500)
            {
                splitContainer1.SplitterDistance = splitContainer1.Width - 500;
            }
        }

        private void Back()
        {
            var service = (INavigationService)GetService(typeof(INavigationService));
            service?.NavigateBack(1);
        }

        protected override TaskListCollection Tasks
        {
            get
            {
                if (_taskList == null)
                {
                    _taskList = new PageTaskList(this);
                }

                base.Tasks.Add(_feature.GetTaskList());
                base.Tasks.Add(_taskList);
                return base.Tasks;
            }
        }

        public IModulePage ParentPage { get; set; }

        private void ListView1MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                _feature.Edit();
            }
        }
    }
}
