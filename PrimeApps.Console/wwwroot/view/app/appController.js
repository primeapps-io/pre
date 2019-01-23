'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', 'ngToast', '$state', '$cookies', '$http', 'config', '$localStorage', 'LayoutService', '$q', '$window',
        function ($rootScope, $scope, $filter, ngToast, $state, $cookies, $http, config, $localStorage, LayoutService, $q, $window) {


            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;

            if (!$rootScope.currentAppId) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('studio.allApps');
                return;
            }

            $rootScope.language = 'en';
            $scope.activeMenu = 'app';
            $scope.activeMenuItem = 'overview';
            $scope.tabTitle = 'Overview';

            $scope.getBasicModules = function () {
                LayoutService.getBasicModules().then(function (result) {
                    $scope.modules = result.data;
                });
            };

            $scope.getBasicModules();
        }
    ]);