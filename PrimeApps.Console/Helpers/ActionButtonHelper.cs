﻿using PrimeApps.Console.Models;
using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Linq;

namespace PrimeApps.Console.Helpers
{
    public class ActionButtonHelper
    {
        public static ActionButton CreateEntity(ActionButtonBindingModel actionButtonModel)
        {
            var actionbutton = new ActionButton
            {
                Name = actionButtonModel.Name,
                Type = actionButtonModel.Type,
                Trigger = actionButtonModel.Trigger,
                CssClass = actionButtonModel.CssClass,
                Template = actionButtonModel.Template,
                Url = actionButtonModel.Url,
                ModuleId = actionButtonModel.ModuleId,
                MethodType = actionButtonModel.MethodType,
                Parameters = actionButtonModel.Parameters
            };

            if (actionButtonModel.Permissions != null && actionButtonModel.Permissions.Count > 0)
            {
                actionbutton.Permissions = new List<ActionButtonPermission>();

                foreach (var permissionModel in actionButtonModel.Permissions)
                {
                    var permissionEntity = new ActionButtonPermission
                    {
                        ProfileId = permissionModel.ProfileId,
                        Type = permissionModel.Type
                    };

                    actionbutton.Permissions.Add(permissionEntity);
                }
            }

            return actionbutton;
        }

        public static void UpdateEntity(ActionButtonBindingModel actionButtonModel, ActionButton actionButton)
        {
            actionButton.Name = actionButtonModel.Name;
            actionButton.Type = actionButtonModel.Type;
            actionButton.Trigger = actionButtonModel.Trigger;
            actionButton.CssClass = actionButtonModel.CssClass;
            actionButton.Template = actionButtonModel.Template;
            actionButton.Url = actionButtonModel.Url;
            actionButton.ModuleId = actionButtonModel.ModuleId;
            actionButton.MethodType = actionButtonModel.MethodType;
            actionButton.Parameters = actionButtonModel.Parameters;

            if (actionButtonModel.Permissions != null && actionButtonModel.Permissions.Count > 0)
            {
                //New Permissions
                foreach (var permissionModel in actionButtonModel.Permissions)
                {
                    if (!permissionModel.Id.HasValue)
                    {
                        if (actionButton.Permissions == null)
                            actionButton.Permissions = new List<ActionButtonPermission>();

                        var permissionEntity = new ActionButtonPermission
                        {
                            ProfileId = permissionModel.ProfileId,
                            Type = permissionModel.Type
                        };

                        actionButton.Permissions.Add(permissionEntity);
                    }
                }

                //Existing Permissions
                if (actionButton.Permissions != null && actionButton.Permissions.Count > 0)
                {
                    foreach (var permissionEntity in actionButton.Permissions)
                    {
                        var permissionModel = actionButtonModel.Permissions.FirstOrDefault(x => x.Id == permissionEntity.Id);

                        if (permissionModel == null)
                            continue;

                        permissionEntity.Type = permissionModel.Type;
                    }
                }
            }
        }
    }
}