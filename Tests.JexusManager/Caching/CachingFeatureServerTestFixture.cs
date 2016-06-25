﻿// Copyright (c) Lex Li. All rights reserved.
// 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Tests.Caching
{
    using System;
    using System.ComponentModel.Design;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    using global::JexusManager.Features.Caching;
    using global::JexusManager.Services;

    using Microsoft.Web.Administration;
    using Microsoft.Web.Management.Client;
    using Microsoft.Web.Management.Client.Win32;
    using Microsoft.Web.Management.Server;

    using Moq;

    using Xunit;

    public class CachingFeatureServerTestFixture
    {
        private CachingFeature _feature;

        private ServerManager _server;

        private ServiceContainer _serviceContainer;

        private const string Current = @"applicationHost.config";

        public async Task SetUp()
        {
            const string Original = @"original.config";
            const string OriginalMono = @"original.mono.config";
            if (Helper.IsRunningOnMono())
            {
                File.Copy("Website1/original.config", "Website1/web.config", true);
                File.Copy(OriginalMono, Current, true);
            }
            else
            {
                File.Copy("Website1\\original.config", "Website1\\web.config", true);
                File.Copy(Original, Current, true);
            }

            Environment.SetEnvironmentVariable(
                "JEXUS_TEST_HOME",
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            _server = new ServerManager(Current) { Mode = WorkingMode.IisExpress };

            _serviceContainer = new ServiceContainer();
            _serviceContainer.RemoveService(typeof(IConfigurationService));
            _serviceContainer.RemoveService(typeof(IControlPanel));
            var scope = ManagementScope.Server;
            _serviceContainer.AddService(typeof(IControlPanel), new ControlPanel());
            _serviceContainer.AddService(typeof(IConfigurationService),
                new ConfigurationService(null, _server.GetApplicationHostConfiguration(), scope, _server, null, null, null, null, null));

            _serviceContainer.RemoveService(typeof(IManagementUIService));
            var mock = new Mock<IManagementUIService>();
            mock.Setup(
                action =>
                action.ShowMessage(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButtons>(),
                    It.IsAny<MessageBoxIcon>(),
                    It.IsAny<MessageBoxDefaultButton>())).Returns(DialogResult.Yes);
            _serviceContainer.AddService(typeof(IManagementUIService), mock.Object);

            var module = new CachingModule();
            module.TestInitialize(_serviceContainer, null);

            _feature = new CachingFeature(module);
            _feature.Load();
        }

        [Fact]
        public async void TestBasic()
        {
            await this.SetUp();
            Assert.Equal(1, _feature.Items.Count);
            Assert.Equal(".cs", _feature.Items[0].Extension);
        }

        [Fact]
        public async void TestRemove()
        {
            await this.SetUp();
            const string Expected = @"expected_remove.config";
            const string ExpectedMono = @"expected_remove.mono.config";

            _feature.SelectedItem = _feature.Items[0];
            _feature.Remove();
            Assert.Null(_feature.SelectedItem);
            Assert.Equal(0, _feature.Items.Count);
            XmlAssert.Equal(
                Helper.IsRunningOnMono()
                    ? Path.Combine("Caching", ExpectedMono)
                    : Path.Combine("Caching", Expected),
                Current);
        }

        [Fact]
        public async void TestEdit()
        {
            await this.SetUp();
            const string Expected = @"expected_edit.config";
            const string ExpectedMono = @"expected_edit.mono.config";

            _feature.SelectedItem = _feature.Items[0];
            var item = _feature.SelectedItem;
            item.Extension = ".doc";
            _feature.EditItem(item);
            Assert.NotNull(_feature.SelectedItem);
            Assert.Equal(".doc", _feature.SelectedItem.Extension);
            Assert.Equal(1, _feature.Items.Count);
            XmlAssert.Equal(
                Helper.IsRunningOnMono()
                    ? Path.Combine("Caching", ExpectedMono)
                    : Path.Combine("Caching", Expected),
                Current);
        }

        [Fact]
        public async void TestAdd()
        {
            await this.SetUp();
            const string Expected = @"expected_add.config";
            const string ExpectedMono = @"expected_add.mono.config";

            var item = new CachingItem(null);
            item.Extension = ".txt";
            _feature.AddItem(item);
            Assert.NotNull(_feature.SelectedItem);
            Assert.Equal(".txt", _feature.SelectedItem.Extension);
            Assert.Equal(2, _feature.Items.Count);
            XmlAssert.Equal(
                Helper.IsRunningOnMono()
                    ? Path.Combine("Caching", ExpectedMono)
                    : Path.Combine("Caching", Expected),
                Current);
        }
    }
}
