'use strict';

angular.module('primeapps')

    .controller('AppsController', ['$rootScope', '$scope', 'guidEmpty', 'entityTypes', 'helper', 'config', '$http', '$localStorage', 'operations', '$filter', '$cache', 'activityTypes', 'AppsService', '$window', '$state', '$modal', 'dragularService', '$timeout', '$interval', '$location', 'ngToast', '$stateParams',
        function ($rootScope, $scope, guidEmpty, entityTypes, helper, config, $http, $localStorage, operations, $filter, $cache, activityTypes, AppsService, $window, $state, $modal, dragularService, $timeout, $interval, $location, ngToast, $stateParams) {

            $scope.loading = true;

            $rootScope.currenOrgId = parseInt($stateParams.organizationId);

            if (!$rootScope.currenOrgId) {
                $state.go('studio.allApps');
            }

            if ($rootScope.organizations)
                $rootScope.currentOrganization = $filter('filter')($rootScope.organizations, {id: parseInt($rootScope.currenOrgId)},true)[0];


            $rootScope.breadcrumblist[0] = {title: $rootScope.currentOrganization.name};
            $rootScope.breadcrumblist[1] = {};
            $rootScope.breadcrumblist[2] = {};

            if (!$rootScope.currenOrgId) {
                ngToast.create({content: $filter('translate')('Common.NotFound'), className: 'warning'});
                $state.go('app.allApps');
                return;
            }

            $scope.appsFilter = {
                organization_id: $rootScope.currenOrgId,
                search: null,
                page: null,
                status: 0
            };

            AppsService.getOrganizationApps($scope.appsFilter)
                .then(function (result) {
                    $scope.apps = result.data;
                    $scope.loading = false;

                });
        }
    ]);