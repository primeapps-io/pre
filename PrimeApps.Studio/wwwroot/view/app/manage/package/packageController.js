﻿'use strict';

angular.module('primeapps')

    .controller('PackageController', ['$rootScope', '$scope', '$state', 'PackageService', '$timeout', '$sce', '$location', '$filter', '$localStorage', '$modal', 'ModuleService',
        function ($rootScope, $scope, $state, PackageService, $timeout, $sce, $location, $filter, $localStorage, $modal, ModuleService) {
            $scope.loading = true;
            $scope.activePage = 1;
            $rootScope.runningPackages[$rootScope.currentApp.name] = { status: true };

            $scope.$parent.activeMenu = 'app';
            $scope.$parent.activeMenuItem = 'packages';
            $rootScope.breadcrumblist[2].title = 'Packages';

            $scope.app = $rootScope.currentApp;
            $scope.packageModules = angular.copy($rootScope.appModules);
            $scope.packageModulesRelations = {};
            $scope.errorList = [];
            $scope.package = {};
            $scope.package.allModulesRelations = {};

            PackageService.getActiveProcess()
                .then(function (response) {
                    var activeProcess = response.data;

                    if (activeProcess) {
                        $rootScope.runningPackages[$scope.app.name] = { status: true };
                        $scope.openWS(activeProcess.id);
                    } else {
                        $rootScope.runningPackages[$scope.app.name] = { status: false };
                    }
                })
                .catch(function (response) {
                    $rootScope.runningPackages[$scope.app.name] = { status: false };
                });

            $scope.openWS = function (id) {
                if ($rootScope.sockets && $rootScope.sockets[$scope.app.name] && $rootScope.sockets[$scope.app.name].readyState !== WebSocket.CLOSED)
                    return;

                if (!$rootScope.sockets)
                    $rootScope.sockets = {};

                var isHttps = location.protocol === 'https:';
                $rootScope.sockets[$scope.app.name] = new WebSocket((isHttps ? 'wss' : 'ws') + '://' + location.host + '/log_stream');
                $rootScope.sockets[$scope.app.name].onopen = function (e) {
                    $rootScope.sockets[$scope.app.name].send(JSON.stringify({
                        'X-App-Id': $scope.app.id,
                        'X-Tenant-Id': $rootScope.currentTenantId,
                        'X-Organization-Id': $scope.app.organization_id,
                        'package_id': id
                    }));
                };
                $rootScope.sockets[$scope.app.name].onclose = function (e) {
                    var packagesPageActive = $location.$$path.contains('/packages');

                    if ($rootScope.runningPackages[$scope.app.name] && $rootScope.runningPackages[$scope.app.name].logs && $rootScope.runningPackages[$scope.app.name].logs.contains('********** Package Created **********')) {
                        toastr.success("Your package is ready for app " + $scope.app.label + ".");

                        if (packagesPageActive) {
                            $rootScope.$broadcast('package-created');
                        }

                        $rootScope.runningPackages[$scope.app.name].status = false;
                        $rootScope.runningPackages[$scope.app.name].logs = ""; 
                        $scope.grid.dataSource.read();
                        $timeout(function () {
                            $scope.$apply();
                        });
                    } else {
                        if (id) {
                            PackageService.get(id)
                                .then(function (response) {
                                    if (response.data) {
                                        if (response.data.status !== 'running') {
                                            if (response.data.status === 'succeed') {
                                                toastr.success("Your package is ready for app " + $scope.app.label + "."); 
                                            }
                                            else {
                                                toastr.error("An unexpected error occurred while creating a package for app " + $scope.app.label + ".");
                                            }

                                            if (packagesPageActive) {
                                                $rootScope.$broadcast('package-created');
                                            }
                                            $rootScope.runningPackages[$scope.app.name].status = false;   
                                            $scope.grid.dataSource.read();
                                            $timeout(function () {
                                                $scope.$apply();   
                                            }); 
                                        }
                                        else {
                                            $scope.openWS($scope.packageId);
                                        }
                                    }
                                });
                        }
                    }
                };
                $rootScope.sockets[$scope.app.name].onerror = function (e) {
                    console.log(e);
                    toastr.error($filter('translate')('Common.Error'));
                    $scope.loading = false;
                };
                $rootScope.sockets[$scope.app.name].onmessage = function (e) {
                    if (!$rootScope.runningPackages[$scope.app.name].status) {
                        $rootScope.runningPackages[$scope.app.name].status = true;
                    }

                    $rootScope.runningPackages[$scope.app.name].logs = e.data;
                    $timeout(function () {
                        $scope.$apply();
                    });

                };
            };

            //for (var i=0; i< $scope.packageModules.length;i++) {
            angular.forEach($scope.packageModules, function (module) {
                // var module =  $scope.packageModules[i];
                if (!module.fields)
                    ModuleService.getModuleFields(module.name).then(function (response) {
                        module.fields = response.data;
                        var lookupList = $filter('filter')(module.fields, function (field) {
                            return field.data_type === 'lookup' && field.lookup_type !== 'users' && field.lookup_type !== 'profiles' && field.lookup_type !== 'roles';
                        });

                        $scope.packageModulesRelations[module.name] = [];
                        $scope.package.allModulesRelations[module.name] = [];

                        for (var k = 0; k < lookupList.length; k++) {
                            $scope.packageModulesRelations[module.name].push(lookupList[k]);
                            $scope.package.allModulesRelations[module.name].push({ name: lookupList[k].name, lookup_type: lookupList[k].lookup_type });
                        }
                    });
            });
            //}

            $scope.createPackage = function () {

                $scope.package.protectModules = 'allModules';
                $scope.package.selectedModules = [];
                $scope.errorList = [];

                $scope.packagePopup = $scope.packagePopup || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/manage/package/packagePopup.html',
                    show: false
                });

                $scope.packagePopup.$promise.then(function () {
                    $scope.packagePopup.show();
                });
            };

            $scope.getTime = function (time) {
                return moment(time).format("DD-MM-YYYY HH:mm");
            };

            $scope.asHtml = function () {
                return $sce.trustAsHtml($rootScope.runningPackages[$scope.app.name] ? $rootScope.runningPackages[$scope.app.name].logs : '');
            };

            $scope.getIcon = function (status) {
                switch (status) {
                    case 'running':
                        return $sce.trustAsHtml('<i style="color:#0d6faa;" class="fas fa-clock"></i>');
                    case 'failed':
                        return $sce.trustAsHtml('<i style="color:rgba(218,10,0,1);" class="fas fa-times"></i>');
                    case 'succeed':
                        return $sce.trustAsHtml('<i style="color:rgba(16,124,16,1);" class="fas fa-check"></i>');
                }
            };

            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    //click event.
                }
            };

            var accessToken = $localStorage.read('access_token');

            $scope.mainGridOptions = {
                dataSource: {
                    type: "odata-v4",
                    page: 1,
                    pageSize: 10,
                    serverPaging: true,
                    serverFiltering: true,
                    serverSorting: true,
                    transport: {
                        read: {
                            url: "/api/package/find",
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            },
                            complete: function () {
                                $scope.loadingDeployments = false;
                                $scope.loading = false;
                            },
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                            fields: {
                                Version: { type: "string" },
                                StartTime: { type: "date" },
                                EndTime: { type: "date" },
                                Status: { type: "enums" }
                            }
                        }
                    }

                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                noRecords: true,
                filterable: true,
                filter: function (e) {
                    if (e.filter && e.field !== 'Status') {
                        for (var i = 0; i < e.filter.filters.length; i++) {
                            e.filter.filters[i].ignoreCase = true;
                        }
                    }
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr>';
                    trTemp += '<td> <span>' + '<div style="padding:12px 0px;">' + e.version + '</div>' + '</span></td > ';
                    trTemp += '<td><span>' + $scope.getTime(e.start_time) + '</span></td>';
                    trTemp += '<td> <span>' + $scope.getTime(e.end_time) + '</span></td > ';
                    trTemp += '<td style="text-align: center;">' + $scope.getIcon(e.status) + '</td></tr>';
                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr class="k-alt">';
                    trTemp += '<td> <span>' + '<div style="padding:12px 0px;">' + e.version + '</div>' + '</span></td > ';
                    trTemp += '<td><span>' + $scope.getTime(e.start_time) + '</span></td>';
                    trTemp += '<td> <span>' + $scope.getTime(e.end_time) + '</span></td > ';
                    trTemp += '<td style="text-align: center;">' + $scope.getIcon(e.status) + '</td></tr>';
                    return trTemp;
                },
                pageable: {
                    refresh: true,
                    pageSize: 10,
                    pageSizes: [10, 25, 50, 100],
                    buttonCount: 5,
                    info: true,
                },
                columns: [
                    {
                        field: 'Version',
                        title: 'Version'
                    },
                    {
                        field: 'StartTime',
                        title: 'Start Time',
                        filterable: {
                            ui: function (element) {
                                element.kendoDateTimePicker({
                                    format: '{0: dd-MM-yyyy  hh:mm}'
                                })
                            }
                        }
                    },
                    {
                        field: 'EndTime',
                        title: 'End Time',
                        filterable: {
                            ui: function (element) {
                                element.kendoDateTimePicker({
                                    format: '{0: dd-MM-yyyy  hh:mm}'
                                })
                            }
                        }
                    },
                    {
                        field: 'Status',
                        title: 'Status',
                        values: [
                            { text: 'Running', value: 'Running' },
                            { text: 'Failed', value: 'Failed' },
                            { text: 'Succeed', value: 'Succeed' }
                        ]
                    }]
            };

            $scope.create = function () {

                var copyRelations = angular.copy($scope.package.allModulesRelations);

                if ($scope.package.protectModules === "allModules") {
                    $scope.errorList = [];
                    $scope.package.selectedModules = [];
                    $scope.package.modulesRelations = copyRelations;
                } else {
                    for (var i = 0; i < $scope.package.selectedModules.length; i++) {
                        var selectedModule = $scope.package.selectedModules[i];
                        $scope.package.selectedModules[i] = {};
                        $scope.package.selectedModules[i][selectedModule.name] = copyRelations[selectedModule.name];
                        delete copyRelations[selectedModule.name];
                    }
                    $scope.package.modulesRelations = copyRelations;
                   // delete $scope.package.allModulesRelations;
                }

                PackageService.create($scope.package)
                    .then(function (response) {
                        toastr.success("Package creation started.");
                        $scope.loading = false;
                        $scope.packageId = response.data;
                        $scope.openWS(response.data);

                        if ($location.$$path.contains('/packages'))
                            $scope.grid.dataSource.read();

                        $rootScope.runningPackages[$scope.app.name] = { status: true };
                    })
                    .catch(function (response) {
                        $scope.loading = false;

                        if (response.status === 409) {
                            toastr.error(response.data);
                        } else {
                            toastr.error($filter('translate')('Common.Error'));
                            console.log(response);
                        }
                    }).finally(function () {
                        $scope.packagePopup.hide();
                        $scope.package.selectedModules = [];
                        $scope.package.modulesRelations = {};
                        $scope.errorList = [];
                    });
            };

            $scope.checkModules = function (selectedModules) {

                if (selectedModules.length === 0)
                    $scope.errorList = [];
                else {
                    for (var i = 0; i < selectedModules.length; i++) {

                        var module = selectedModules[i];
                        var index = -1;
                        var relatedModuleIndex = -1;
                        var lookupList = $scope.packageModulesRelations[module.name];

                        for (var o = 0; o < lookupList.length; o++) {

                            var isExistModule = $filter('filter')(selectedModules, { name: lookupList[o].lookup_type }, true)[0];
                            var relatedModule = $filter('filter')($scope.errorList, function (error) {
                                return error.lookup_type === lookupList[o].lookup_type && error.module.name === lookupList[o].module.name;
                            })[0];

                            if (relatedModule)
                                relatedModuleIndex = $scope.errorList.indexOf(relatedModule);

                            index = lookupList.indexOf(lookupList[o]);
                            //if we didn't add that, we will add that in this case
                            if (!isExistModule && index === -1) {
                                $scope.errorList.push(lookupList[o]);
                            }
                            //Seçilen moduller arasında olmayıp,lookup olanı ekliyoruz
                            else if (!isExistModule && index > -1 && relatedModuleIndex === -1) {
                                $scope.errorList.push(lookupList[o]);
                            }
                            //if we added that before we have to splice that from array 
                            else if (isExistModule && index > -1 && relatedModuleIndex > -1) {
                                $scope.errorList.splice(relatedModuleIndex, 1);
                            }
                        }

                        for (var j = 0; j < $scope.errorList.length; j++) {
                            var isExistInSelectedModules = $filter('filter')(selectedModules, { name: $scope.errorList[j].module.name }, true)[0];
                            if (!isExistInSelectedModules) {
                                $scope.errorList.splice(j, 1);
                            }
                        }
                    }
                }
            };

            $scope.getErrorText = function (moduleName) {
                var module = $filter('filter')($scope.packageModules, { name: moduleName }, true)[0];
                return module["label_" + $rootScope.language + "_plural"];
            };
        }
    ]);