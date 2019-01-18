'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', 'ngToast', '$state', '$cookies', '$http', 'config', '$localStorage', 'LayoutService', '$q',
        function ($rootScope, $scope, $filter, ngToast, $state, $cookies, $http, config, $localStorage, LayoutService, $q) {


            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;

            if (!$scope.appId) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.allApps');
                return;
            }

            $cookies.put('app_id', $scope.appId);

            if ($scope.appId != ($localStorage.get("current_app") != null ? $localStorage.get("current_app").id : false)) {
                $http.get(config.apiUrl + "app/get/" + $scope.appId).then(function (result) {
                    if (result.data) {
                        $scope.menuTopTitle = result.data.label;
                        $localStorage.set("current_app", result.data);
                        $rootScope.breadcrumbListe[1].title = result.data.label;
                        $rootScope.breadcrumbListe[1].link = '#/app/' + $scope.appId + '/overview';
                    }
                });
            } else {
                $scope.setTopTitle = function (link) {
                    $scope.menuTopTitle = $localStorage.get("current_app").label;
                    $rootScope.breadcrumbListe[1].title = $scope.menuTopTitle;
                    $rootScope.breadcrumbListe[1].link = '#/app/' + $scope.appId + '/' + link;
                }
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