'use strict';

angular.module('primeapps')

    .controller('SingleSingOnController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'SingleSingOnService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService,SingleSingOnService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Identity";
            $scope.$parent.activeMenu = 'identity';
            $scope.$parent.activeMenuItem = 'singleSingOn';
            console.log("SingleSingOnController");

        }
    ]);