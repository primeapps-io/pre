'use strict';

angular.module('primeapps')
    .factory('SettingService', ['$http', 'config',
        function ($http, config) {
            return {

                editUser: function (user) {
                    return $http.put(config.apiUrl + 'user/edit', {
                        //id: user.id,
                        first_name: user.firstName,
                        last_name: user.lastName,
                        email: user.email,
                        // password: user.password,
                        //picture: user.picture,
                        // phone: user.phone
                    });
                },

                removeUser: function (password) {
                    return $http.post(config.apiUrl + 'user/change_password', angular.toJson(password));
                },

                changePassword: function (currentPassword, newPassword, confirmPassword) {
                    return $http.post(config.apiUrl + 'account/change_password', {
                        old_password: currentPassword,
                        new_password: newPassword,
                        confirm_password: confirmPassword
                    });
                }

            };
        }
    ]);