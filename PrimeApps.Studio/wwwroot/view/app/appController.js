'use strict';

angular.module('primeapps')

    .controller('AppController', ['$rootScope', '$scope', '$filter', '$state', '$cookies', '$http', 'config', '$localStorage', 'LayoutService', '$q', '$window',
        function ($rootScope, $scope, $filter, $state, $cookies, $http, config, $localStorage, LayoutService, $q, $window) {


            $scope.appId = $state.params.appId;
            $scope.orgId = $state.params.orgId;
            $rootScope.menuOpen = [];
            $rootScope.menuOpen[$scope.orgId] = true;
            $rootScope.subMenuOpen = "";

            if (!$rootScope.currentAppId) {
                toastr.warning($filter('translate')('Common.NotFound'));
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

            $rootScope.openSubMenu = function (name) {
                if ($rootScope.subMenuOpen == name)
                    $rootScope.subMenuOpen = "";
                else
                    $rootScope.subMenuOpen = name;
                $scope.activeMenuItem = "";
            }
            // setTimeout( function() {
            //     $('.setup-nav > li').each(function(){
            //         var t = null;
            //         var li = $(this);
            //         li.hover(function(){
            //             t = setTimeout(function(){
            //                 li.find("ul").slideDown(300);
            //                 t = null;
            //             }, 300);
            //         }, function(){
            //             if (t){
            //                 clearTimeout(t);
            //                 t = null;
            //             }
            //             else
            //                 li.find("ul").slideUp(200);
            //         });
            //     });
            // }, 200);

                
           
        }
    ]);