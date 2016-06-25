﻿// Copyright (c) Lex Li. All rights reserved.
// 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace JexusManager.Wizards.ConnectionWizard
{
    using System;
    using System.IO;

    using Microsoft.Web.Administration;
    using Microsoft.Web.Management.Client.Win32;
    using System.Windows.Forms;

    public partial class BrowsePage : WizardPage
    {
        private bool _initialized;

        public BrowsePage()
        {
            InitializeComponent();
            Caption = "Specify a Configuration File";
        }

        protected internal override bool CanNavigateNext
        {
            get
            {
                return base.CanNavigateNext && File.Exists(txtName.Text);
            }
        }

        private void TxtNameTextChanged(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            ((ConnectionWizardData)WizardData).FileName = txtName.Text;
            this.UpdateWizard();
        }

        public override bool OnNext()
        {
            var data = ((ConnectionWizardData)WizardData);
            if (data.FileName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                var folder = Path.GetDirectoryName(data.FileName);
                var config = Path.Combine(folder, ".vs", "config", "applicationHost.config");
                if (File.Exists(config))
                {
                    data.Server = new ServerManager(config);
                    data.FileName = config;
                }
                else
                {
                    var service = (IManagementUIService)GetService(typeof(IManagementUIService));
                    service.ShowMessage("This solution does not contain IIS Express configuration file.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                data.Server = new ServerManager(data.FileName);
            }

            data.Server.Mode = WorkingMode.IisExpress;
            return true;
        }

        protected internal override void Activate()
        {
            base.Activate();
            _initialized = false;
            txtName.Text = ((ConnectionWizardData)this.WizardData).FileName;
            _initialized = true;
            txtName.Focus();
            txtName.SelectAll();
        }

        private void BtnBrowseClick(object sender, EventArgs e)
        {
            DialogHelper.ShowFileDialog(txtName, "Common Files|*.config;*.sln|Config Files|*.config|Solution Files|*.sln|All Files|*.*");
        }
    }
}
