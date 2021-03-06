'use strict';

angular.module('primeapps')

    .factory('RoleService', ['$http', 'config',
        function ($http, config) {
            return {

                getAll: function () {
                    return $http.post(config.apiUrl + 'role/find_all', {});
                },

                create: function (role) {
                    return $http.post(config.apiUrl + 'role/create', role);
                },

                update: function (role, role_change) {
                    return $http.put(config.apiUrl + 'role/update?roleChange=' + role_change, role);
                },

                delete: function (id, transferRoleId) {
                    return $http.delete(config.apiUrl + 'role/delete?id=' + id + '&transferRoleId=' + transferRoleId);
                },

                updateUserRole: function (userId, roleId) {
                    return $http.put(config.apiUrl + 'role/update_user_role?user_Id=' + userId + '&role_Id=' + roleId);
                },

                roleChangeUsers: function (userIdList, roleId) {
                    return $http.post(config.apiUrl + 'role/role_change_users', { user_id_list: userIdList, role_id: roleId });
                }
            };
        }]);