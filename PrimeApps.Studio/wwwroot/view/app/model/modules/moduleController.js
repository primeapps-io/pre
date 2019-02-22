﻿'use strict';

angular.module('primeapps')

    .controller('ModuleController', ['$rootScope', '$scope', '$filter', '$state', '$dropdown', '$modal', 'helper', 'ModuleService', '$cache', 'LayoutService',
        function ($rootScope, $scope, $filter, $state, $dropdown, $modal, helper, ModuleService, $cache, LayoutService) {

            //$scope.$parent.menuTopTitle = "Models";

            $scope.$parent.activeMenuItem = 'modules';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };

            $scope.generator(10);
            $rootScope.breadcrumblist[2].title = 'Modules';

            $scope.modules = [];
            $scope.loading = true;
            $scope.requestModel = {
                limit: "10",
                offset: 0
            };

            $scope.activePage = 1;

            ModuleService.count().then(function (response) {
                $scope.pageTotal = response.data;
                $rootScope.appModules.length = response.data;
                $scope.changePage(1);

            });


            $scope.changePage = function (page, deleted) {
                $scope.loading = true;

                if (page > Math.ceil($scope.pageTotal / $scope.requestModel.limit)) {
                    --page;
                }

                $scope.activePage = page;
                var requestModel = angular.copy($scope.requestModel);
                requestModel.offset = page - 1;

                ModuleService.find(requestModel).then(function (response) {
                    $scope.modules = response.data;
                    $scope.loading = false;
                });

            };

            $scope.changeOffset = function () {
                $scope.changePage($scope.activePage, true)
            };

            $scope.delete = function (module, event) {
                var willDelete =
                    swal({
                        title: "Are you sure?",
                        text: " ",
                        icon: "warning",
                        buttons: ['Cancel', 'Yes'],
                        dangerMode: true
                    }).then(function (value) {
                        if (value) {

                            var elem = angular.element(event.srcElement);
                            angular.element(elem.closest('tr')).addClass('animated-background');
                            ModuleService.delete(module.id)
                                .then(function () {

                                    $scope.pageTotal--;
                                    $rootScope.appModules.length = $scope.pageTotal;
                                    angular.element(elem.closest('tr')).remove();
                                    $scope.changePage($scope.activePage, true);
                                    toastr.success("Module is deleted successfully.", "Deleted!");

                                })
                                .catch(function () {

                                });

                        }
                    });
            };
        }
    ]);