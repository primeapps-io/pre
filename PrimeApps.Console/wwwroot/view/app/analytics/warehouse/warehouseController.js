'use strict';

angular.module('primeapps')

    .controller('WarehouseController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'WarehouseService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, WarehouseService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            //$scope.$parent.menuTopTitle = "Analytics";
            // $scope.$parent.activeMenu = 'analytics';
            $scope.$parent.activeMenuItem = 'warehouse';
            $rootScope.breadcrumblist[2].title = 'Warehouse';

            console.log("WarehouseController");

        }
    ]);