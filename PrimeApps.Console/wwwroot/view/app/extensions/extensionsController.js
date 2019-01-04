'use strict';

angular.module('primeapps')

    .controller('ExtensionsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', 'ngToast', '$modal', '$timeout', 'helper', 'dragularService', 'ExtensionsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, ngToast, $modal, $timeout, helper, dragularService, ExtensionsService, LayoutService, $http, config) {

            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            $scope.$parent.menuTopTitle = "Xbrand";
            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'extensions';

            console.log("ExtensionsController");

        }
    ]);