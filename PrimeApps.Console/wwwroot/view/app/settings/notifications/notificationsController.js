'use strict';

angular.module('primeapps')

    .controller('NotificationsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'NotificationsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, NotificationsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            //$scope.$parent.menuTopTitle = "Settings";
           //$scope.$parent.activeMenu = 'settings';
            $scope.$parent.activeMenuItem = 'notifications';

            $rootScope.breadcrumblist[2].title = 'Notifications';


            console.log("NotificationsController");

        }
    ]);