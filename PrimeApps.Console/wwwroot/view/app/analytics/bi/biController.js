'use strict';

angular.module('primeapps')

    .controller('BiController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'BiService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,BiService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Analytics";
            $scope.$parent.activeMenu = 'analytics';
            $scope.$parent.activeMenuItem = 'bi';

            console.log("BiController");

        }
    ]);