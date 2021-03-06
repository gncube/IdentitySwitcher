﻿#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//

#endregion

namespace DNN.Modules.IdentitySwitcher.Components
{
    using System.IO;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using global::IdentitySwitcher.DotNetNuke.Web.Client;

    /// <summary>
    /// </summary>
    /// <seealso cref="DotNetNuke.Entities.Modules.PortalModuleBase" />
    public abstract class IdentitySwitcherPortalModuleBase : PortalModuleBase
    {
        /// <summary>
        ///     The distribution folder
        /// </summary>
        public const string DistributionFolder = "dist";

        /// <summary>
        ///     The module folder name
        /// </summary>
        private string _moduleFolderName;

        /// <summary>
        ///     Gets the name of the script folder.
        /// </summary>
        /// <value>
        ///     The name of the script folder.
        /// </value>
        public virtual string ScriptFolderName { get; } = "Scripts";

        /// <summary>
        ///     Gets the name of the js resources folder.
        /// </summary>
        /// <value>
        ///     The name of the js resources folder.
        /// </value>
        public virtual string JsResourcesFolderName { get; } = "jsResources";

        /// <summary>
        ///     Gets the name of the module angular application folder.
        /// </summary>
        /// <value>
        ///     The name of the module angular application folder.
        /// </value>
        public virtual string ModuleAngularAppFolderName => Path.Combine(
            this.ModuleFolderName, this.ScriptFolderName, "app");

        /// <summary>
        ///     Gets the name of the module folder.
        /// </summary>
        /// <value>
        ///     The name of the module folder.
        /// </value>
        public virtual string ModuleFolderName
        {
            get
                {
                    if (string.IsNullOrWhiteSpace(this._moduleFolderName))
                    {
                        this._moduleFolderName =
                            Path.Combine(Globals.DesktopModulePath, this.ModuleConfiguration.DesktopModule.FolderName);
                    }
                    return this._moduleFolderName;
                }
        }

        /// <summary>
        ///     Gets the module script folder.
        /// </summary>
        /// <value>
        ///     The module script folder.
        /// </value>
        protected virtual string ModuleScriptFolder => Path.Combine(this.ModuleFolderName, this.ScriptFolderName);

        /// <summary>
        ///     Gets the module js resources folder.
        /// </summary>
        /// <value>
        ///     The module js resources folder.
        /// </value>
        protected virtual string ModuleJsResourcesFolder => Path.Combine(
            this.ModuleFolderName, this.JsResourcesFolderName);

        /// <summary>
        ///     Gets the module instance.
        /// </summary>
        /// <returns></returns>
        protected virtual ModuleInstanceBase GetModuleInstance()
        {
            return IdentitySwitcherClient.GetModuleInstance<ModuleInstanceBase>(this);
        }

        /// <summary>
        ///     Registers the script.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="priority">The priority.</param>
        protected void RegisterScript(string folder, string fileName, IdentitySwitcherFileOrder.Js priority)
        {
            var scriptPath = string.IsNullOrWhiteSpace(folder) ? fileName : Path.Combine(folder, fileName);
            ClientResourceManager.RegisterScript(this.Page, scriptPath, (int) priority);
        }
    }
}