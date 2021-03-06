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
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Http;
    using DNN.Modules.IdentitySwitcher.Components.Model;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// </summary>
    /// <seealso cref="DotNetNuke.Web.Api.DnnApiController" />
    public class IdentitySwitcherController : DnnApiController
    {
        /// <summary>
        ///     Gets or sets the users.
        /// </summary>
        /// <value>
        ///     The users.
        /// </value>
        private List<UserInfo> Users { get; set; }

        /// <summary>
        ///     Gets or sets the module identifier.
        /// </summary>
        /// <value>
        ///     The module identifier.
        /// </value>
        private int ModuleID { get; set; }

        /// <summary>
        ///     Switches the user.
        /// </summary>
        /// <param name="selectedUserId">The selected user identifier.</param>
        /// <param name="selectedUserUserName">Name of the selected user user.</param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public IHttpActionResult SwitchUser(int selectedUserId, string selectedUserUserName)
        {
            if (selectedUserId == -1)
            {
                HttpContext.Current.Response.Redirect(Globals.NavigateURL("LogOff"));
            }
            else
            {
                var MyUserInfo = UserController.GetUserById(this.PortalSettings.PortalId, selectedUserId);


                DataCache.ClearUserCache(this.PortalSettings.PortalId, selectedUserUserName);


                // sign current user out
                var objPortalSecurity = new PortalSecurity();
                objPortalSecurity.SignOut();

                // sign new user in
                UserController.UserLogin(this.PortalSettings.PortalId, MyUserInfo, this.PortalSettings.PortalName,
                                         HttpContext.Current.Request.UserHostAddress, false);
            }
            return this.Ok();
        }

        /// <summary>
        ///     Gets the search items.
        /// </summary>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public IHttpActionResult GetSearchItems()
        {
            var result = new List<string>();

            var profileProperties =
                ProfileController.GetPropertyDefinitionsByPortal(this.PortalSettings.PortalId, false);

            foreach (ProfilePropertyDefinition definition in profileProperties)
            {
                result.Add(definition.PropertyName);
            }
            result.AddRange(new List<string> {"RoleName", "Email", "Username"});

            return this.Ok(result);
        }

        /// <summary>
        ///     Gets the users.
        /// </summary>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="searchText">The search text.</param>
        /// <param name="selectedSearchItem">The selected search item.</param>
        /// <returns></returns>
        [DnnAuthorize]
        [HttpGet]
        public IHttpActionResult GetUsers(int moduleId, string searchText = null, string selectedSearchItem = null)
        {
            this.ModuleID = moduleId;

            if (searchText == null)
            {
                this.LoadAllUsers();
            }
            else
            {
                this.Filter(searchText, selectedSearchItem);
            }

            var result = this.Users.Select(userInfo => new UserDto
                                                           {
                                                               Id = userInfo.UserID,
                                                               UserName = userInfo.Username,
                                                               UserAndDisplayName = userInfo.DisplayName != null
                                                                                        ? $"{userInfo.DisplayName} - {userInfo.Username}"
                                                                                        : userInfo.Username
                                                           })
                             .ToList();

            return this.Ok(result);
        }

        /// <summary>
        ///     Loads all users.
        /// </summary>
        private void LoadAllUsers()
        {
            this.Users = UserController.GetUsers(this.PortalSettings.PortalId).OfType<UserInfo>().ToList();
            this.SortUsers();

            this.LoadDefaultUsers();
        }

        /// <summary>
        ///     Loads the default users.
        /// </summary>
        private void LoadDefaultUsers()
        {
            var moduleInfo = new ModuleController().GetModule(this.ModuleID);
            var repository = new IdentitySwitcherModuleSettingsRepository();
            var settings = repository.GetSettings(moduleInfo);

            if (settings.IncludeHost != null && (bool) settings.IncludeHost)
            {
                var arHostUsers = UserController.GetUsers(Null.NullInteger);

                foreach (UserInfo hostUser in arHostUsers)
                {
                    this.Users.Insert(
                        0,
                        new UserInfo {Username = hostUser.Username, UserID = hostUser.UserID, DisplayName = null});
                }
            }

            this.Users.Insert(0, new UserInfo {Username = "Anonymous", DisplayName = null});
        }

        /// <summary>
        ///     Sorts the users.
        /// </summary>
        private void SortUsers()
        {
            var moduleInfo = new ModuleController().GetModule(this.ModuleID);
            var repository = new IdentitySwitcherModuleSettingsRepository();
            var settings = repository.GetSettings(moduleInfo);

            switch (settings.SortBy)
            {
                case SortBy.DisplayName:
                    this.Users = this.Users.OrderBy(arg => arg.DisplayName.ToLower()).ToList();
                    break;
                case SortBy.UserName:
                    this.Users = this.Users.OrderBy(arg => arg.Username.ToLower()).ToList();
                    break;
            }
        }

        /// <summary>
        ///     Filters the specified search text.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="selectedSearchItem">The selected search item.</param>
        private void Filter(string searchText, string selectedSearchItem)
        {
            var total = 0;

            switch (selectedSearchItem)
            {
                case "Email":
                    this.Users = UserController
                        .GetUsersByEmail(this.PortalSettings.PortalId, searchText + "%", -1, -1, ref total)
                        .OfType<UserInfo>().ToList();
                    break;
                case "Username":
                    this.Users = UserController
                        .GetUsersByUserName(this.PortalSettings.PortalId, searchText + "%", -1, -1, ref total)
                        .OfType<UserInfo>().ToList();
                    break;
                case "RoleName":
                    this.Users = RoleController
                        .Instance.GetUsersByRole(this.PortalSettings.PortalId, searchText).ToList();
                    break;

                default:
                    this.Users = UserController
                        .GetUsersByProfileProperty(this.PortalSettings.PortalId, selectedSearchItem, searchText + "%",
                                                   0, 1000, ref total)
                        .OfType<UserInfo>().ToList();
                    break;
            }
            this.SortUsers();

            this.LoadDefaultUsers();
        }
    }
}