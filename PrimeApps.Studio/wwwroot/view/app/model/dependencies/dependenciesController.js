﻿'use strict';

angular.module('primeapps')

    .controller('DependenciesController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', 'helper', '$cache', 'systemRequiredFields', 'systemReadonlyFields', 'DependenciesService', 'LayoutService', 'ModuleService', '$timeout', '$location', '$localStorage',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, helper, $cache, systemRequiredFields, systemReadonlyFields, DependenciesService, LayoutService, ModuleService, $timeout, $location, $localStorage) {

            //$scope.$parent.menuTopTitle = "Models";
            //$scope.$parent.activeMenu = "model";
            $scope.$parent.activeMenuItem = "dependencies";
            $rootScope.breadcrumblist[2].title = 'Dependencies';
            $scope.dependencies = [];

            $scope.id = $location.search().id ? $location.search().id : 0;
  
            $scope.moduleChanged = function () {

                $scope.parentDisplayFields = [];
                $scope.parentValueFields = [];
                $scope.childValueListFields = [];
                $scope.childValueTextFields = [];
                $scope.childDisplayFields = [];
                $scope.picklistFields = [];

                if ($scope.currentDependency.module)
                    if (!$scope.currentDependency.module.fields) {
                        var moduleName = $scope.currentDependency.module.name;
                        ModuleService.getModuleByName(moduleName).then(function (response) {
                            $scope.module = response.data;
                            $scope.sections = $scope.module.sections;
                            prepareDependency();
                        });
                    } else {
                        $scope.module = angular.copy($scope.currentDependency.module);
                        $scope.sections = $scope.currentDependency.module.sections;
                        prepareDependency();
                    }
                $scope.modalLoading = false;
            };
            $scope.affectedAreaType = "field";

            var getFields = function () {

                $scope.dependencies = $scope.grid.dataSource.data();

                angular.forEach($scope.module.fields, function (field) {

                    //It was closed for lookup fields appear.
                    //if (isSystemField(field))
                    //    return;

                    var existDisplayDependency = $filter('filter')($scope.dependencies, {
                        child_field: { name: field.name },
                        dependency_type: 'display'
                    }, true)[0];
                    var existValueDependency = $filter('filter')($scope.dependencies, {
                        child_field: { name: field.name },
                        dependency_type: 'value'
                    }, true)[0];

                    if (!existDisplayDependency)
                        $scope.childDisplayFields.push(field);

                    if (!existValueDependency) {
                        switch (field.data_type) {
                            case 'picklist':
                                $scope.parentDisplayFields.push(field);
                                $scope.parentValueFields.push(field);
                                $scope.childValueListFields.push(field);
                                break;
                            case 'multiselect':
                                $scope.parentDisplayFields.push(field);
                                $scope.childValueListFields.push(field);
                                break;
                            case 'lookup':
                                $scope.childValueListFields.push(field);
                                break;
                            case 'checkbox':
                                $scope.parentDisplayFields.push(field);
                                $scope.childValueListFields.push(field);
                                break;
                            case 'text_single':
                            case 'text_multi':
                            case 'number':
                            case 'number_decimal':
                            case 'currency':
                            case 'email':
                                $scope.childValueTextFields.push(field);
                                break;
                        }
                    }

                    if (field.data_type === 'picklist')
                        $scope.picklistFields.push(field);
                });

                function isSystemField(field) {
                    if (systemRequiredFields.all.indexOf(field.name) > -1 || (systemRequiredFields[$scope.module.name] && systemRequiredFields[$scope.module.name].indexOf(field.name) > -1))
                        return true;

                    return false;
                }
            };

            var getDependencyTypes = function () {
                var dependencyTypeDisplay = {};
                dependencyTypeDisplay.value = 'display';
                dependencyTypeDisplay.label = $filter('translate')('Setup.Modules.DependencyTypeDisplay');

                var dependencyTypeValueChange = {};
                dependencyTypeValueChange.value = 'value';
                dependencyTypeValueChange.label = $filter('translate')('Setup.Modules.DependencyTypeValueChange');

                var dependencyTypeFreeze = {};
                dependencyTypeFreeze.value = 'freeze';
                dependencyTypeFreeze.label = $filter('translate')('Setup.Modules.DependencyTypeFreeze');

                $scope.dependencyTypes = [];
                $scope.dependencyTypes.push(dependencyTypeDisplay);
                $scope.dependencyTypes.push(dependencyTypeValueChange);
                $scope.dependencyTypes.push(dependencyTypeFreeze);
            };
            getDependencyTypes();

            var getValueChangeTypes = function () {
                var valueChangeTypeStandard = {};
                valueChangeTypeStandard.value = 'list_text';
                valueChangeTypeStandard.label = $filter('translate')('Setup.Modules.ValueChangeTypeStandard');

                var valueChangeTypeValueMapping = {};
                valueChangeTypeValueMapping.value = 'list_value';
                valueChangeTypeValueMapping.label = $filter('translate')('Setup.Modules.ValueChangeTypeValueMapping');

                var valueChangeTypeFieldMapping = {};
                valueChangeTypeFieldMapping.value = 'list_field';
                valueChangeTypeFieldMapping.label = $filter('translate')('Setup.Modules.ValueChangeTypeFieldMapping');

                $scope.valueChangeTypes = [];
                $scope.valueChangeTypes.push(valueChangeTypeStandard);
                $scope.valueChangeTypes.push(valueChangeTypeValueMapping);
                $scope.valueChangeTypes.push(valueChangeTypeFieldMapping);
            };

            $scope.dependencyTypeChanged = function () {
                if ($scope.currentDependency.dependencyType === 'value') {
                    $scope.currentDependency.type = 'list_text';
                }

                $scope.currentDependency.parent_field = null;
                $scope.currentDependency.child_field = null;
                $scope.currentDependency.child_section = null;
            };

            $scope.valueChangeTypeChanged = function () {
                switch ($scope.currentDependency.type) {
                    case 'list_value':
                        $scope.currentDependency.value_maps = {};
                        break;
                    case 'list_field':
                        $scope.currentDependency.field_map = {};
                        break;
                }
            };

            $scope.getParentFields = function () {
                if ($scope.currentDependency)
                    switch ($scope.currentDependency.dependencyType) {
                        case 'display':
                            return $scope.parentDisplayFields;
                        case 'freeze':
                            return $scope.parentDisplayFields;
                            break;
                        case 'value':
                            if ($scope.currentDependency.type === 'list_value')
                                return $scope.picklistFields;
                            else
                                return $scope.parentValueFields;
                    }
            };

            $scope.getChildFields = function () {
                if ($scope.currentDependency)
                    switch ($scope.currentDependency.dependencyType) {
                        case 'display':
                            angular.forEach($scope.childDisplayFields, function (field) {
                                delete field.hidden;

                                //Silinen alanlar alan bagimliklarinda gelmeye devam ediyordu.
                                if (field.name === $scope.currentDependency.parent_field || field.deleted)
                                    field.hidden = true;
                            });

                            return $scope.childDisplayFields;
                        case 'freeze':
                            angular.forEach($scope.childDisplayFields, function (field) {
                                delete field.hidden;

                                //Silinen alanlar alan bagimliklarinda gelmeye devam ediyordu.
                                if (field.name === $scope.currentDependency.parent_field || field.deleted)
                                    field.hidden = true;
                            });

                            return $scope.childDisplayFields;
                            break;
                        case 'value':
                            if ($scope.currentDependency.type === 'list_text') {
                                angular.forEach($scope.childValueTextFields, function (field) {
                                    delete field.hidden;

                                    if (field.name === $scope.currentDependency.parent_field)
                                        field.hidden = true;
                                });

                                return $scope.childValueTextFields;
                            } else {
                                angular.forEach($scope.childValueListFields, function (field) {
                                    delete field.hidden;

                                    if (field.name === $scope.currentDependency.parent_field)
                                        field.hidden = true;
                                });

                                return $scope.childValueListFields;
                            }
                    }
            };

            $scope.getMappingOptions = function () {
                var parentField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.parent_field }, true)[0];
                var childField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.child_field }, true)[0];
                var childSection = $filter('filter')($scope.module.sections, { name: $scope.currentDependency.child_section }, true)[0];
                $scope.parentPicklist = [];
                if (parentField.picklist_id) {
                    DependenciesService.getPicklist(parentField.picklist_id).then(function (picklists) {
                        var copyPicklist = angular.copy(picklists.data.items);
                        $scope.parentPicklist = $filter('filter')(copyPicklist, { inactive: '!true' });
                        if (childField.picklist_id) {
                            DependenciesService.getPicklist(childField.picklist_id).then(function (picklists) {
                                var copyPicklist = angular.copy(picklists.data.items);
                                var childPicklist = $filter('filter')(copyPicklist, { inactive: '!true' });
                                angular.forEach($scope.parentPicklist, function (picklistItem) {
                                    picklistItem.childPicklist = childPicklist;
                                });
                            });
                        }
                        return $scope.parentPicklist;
                    }
                    );
                }
            };

            var setCurrentDependency = function (dependency) {
                $scope.currentDependency = {};
                $scope.currentDependency = angular.copy(dependency);
                $scope.currentDependency.hasRelationField = true;
                //$scope.currentDependency.module = dependency.parent_module;
                $scope.currentDependencyState = angular.copy($scope.currentDependency);
            };

            $scope.showFormModal = function (dependency) {

                $scope.parentDisplayFields = [];
                $scope.parentValueFields = [];
                $scope.childValueListFields = [];
                $scope.childValueTextFields = [];
                $scope.childDisplayFields = [];
                $scope.picklistFields = [];

                if (!dependency) {
                    dependency = {};
                    dependency.dependencyType = 'display';
                    dependency.isNew = true;
                    setCurrentDependency(dependency);
                } else {
                    $scope.modalLoading = true;

                    DependenciesService.getDependency(dependency.id).then(function (response) {
                        var dependency = DependenciesService.processDependencies(response.data);
                        setCurrentDependency(dependency);
                        $scope.dependencies = $scope.grid.dataSource.data();
                        $scope.moduleChanged();
                        $scope.getMappingOptions();
                    });
                }

                $scope.addNewDependencyModal = $scope.addNewDependencyModal || $modal({
                    scope: $scope,
                    templateUrl: 'view/app/model/dependencies/dependencyForm.html',
                    animation: 'am-fade-and-slide-right',
                    backdrop: 'static',
                    show: false
                });

                $scope.addNewDependencyModal.$promise.then(function () {
                    $scope.addNewDependencyModal.show();
                });
            };

            $scope.save = function (dependencyForm) {

                if (!dependencyForm.$valid) {
                    if (dependencyForm.$error.required)
                        toastr.error($filter('translate')('Setup.Modules.RequiredError'));
                    return;
                }


                $scope.saving = true;
                var dependency = angular.copy($scope.currentDependency);
                if (dependency.isNew) {
                    delete dependency.isNew;

                    if (!$scope.dependencies)
                        $scope.dependencies = [];

                    //  $scope.dependencies.push(dependency);
                }
                var dependencyModel = DependenciesService.prepareDependency(angular.copy(dependency), $scope.module);

                var success = function () {
                    //$scope.loading = true;
                    toastr.success($filter('translate')('Setup.Modules.DependencySaveSuccess'));
                    $scope.saving = false;
                    $scope.addNewDependencyModal.hide();
                    $scope.grid.dataSource.read();
                };

                var error = function () {

                    if ($scope.addNewDependencyModal) {
                        $scope.addNewDependencyModal.hide();
                        $scope.saving = false;
                    }
                };

                if (!dependencyModel.id) {
                    DependenciesService.createModuleDependency(dependencyModel, $scope.module.id)
                        .then(function () {
                            success();
                            $scope.pageTotal++;
                        })
                        .catch(function () {
                            error();
                        });
                } else {
                    DependenciesService.updateModuleDependency(dependencyModel, $scope.module.id)
                        .then(function () {
                            success();
                        })
                        .catch(function () {
                            error();
                        });
                }
            };

            $scope.delete = function (dependency, event) {
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


                            DependenciesService.deleteModuleDependency(dependency.id)
                                .then(function () {
                                    // var dependencyIndex = helper.arrayObjectIndexOf($scope.dependencies, dependency);
                                    // $scope.dependencies.splice(dependencyIndex, 1); 
                                    toastr.success("Dependency is deleted successfully.", "Deleted!");
                                    $scope.grid.dataSource.read();
                                })
                                .catch(function () {
                                    if ($scope.addNewDependencyModal) {
                                        $scope.addNewDependencyModal.hide();
                                        $scope.saving = false;
                                    }
                                });
                        }
                    })
            };

            $scope.parentValueChanged = function () {
                $scope.currentDependency.values = [];
                $scope.getPicklist();
            };

            $scope.getPicklist = function () {
                $scope.picklist = [];
                var parentField = $filter('filter')($scope.module.fields, { name: $scope.currentDependency.parent_field }, true)[0];
                if (parentField.picklist_id) {
                    DependenciesService.getPicklist(parentField.picklist_id).then(function (picklists) {
                        var copyPicklist = angular.copy(picklists.data.items);
                        $scope.picklist = $filter('filter')(copyPicklist, { inactive: '!true' });
                    }).finally(function () {
                        $scope.modalLoading = false;
                    });
                }
            };

            var prepareDependency = function () {
                getFields();
                // getDependencyTypes();
                getValueChangeTypes();
                var dependency = $scope.currentDependency;
                if (!dependency.isNew) {
                    var childField = $filter('filter')($scope.module.fields, { name: dependency.child_field }, true)[0];
                    var sectionField = $filter('filter')($scope.module.sections, { name: dependency.child_section }, true)[0];
                    $scope.affectedAreaType = dependency.child_section ? 'section' : 'field';

                    var childValueListFieldsExist = $filter('filter')($scope.childValueListFields, { name: childField.name }, true)[0];
                    if (!childValueListFieldsExist)
                        $scope.childValueListFields.push(childField);

                    var childValueTextFieldsExist = $filter('filter')($scope.childValueTextFields, { name: childField.name }, true)[0];
                    if (!childValueTextFieldsExist)
                        $scope.childValueTextFields.push(childField);

                    var childDisplayFieldExist = $filter('filter')($scope.childDisplayFields, { name: childField.name }, true)[0];
                    if (!childDisplayFieldExist)
                        $scope.childDisplayFields.push(childField);

                    $scope.getPicklist();
                }
            };

            //For Kendo UI
            $scope.goUrl = function (item) {
                var selection = window.getSelection();
                if (selection.toString().length === 0) {
                    $scope.showFormModal(item); //click event.
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
                            url: "/api/dependency/find/" + $scope.id,
                            type: 'GET',
                            dataType: "json",
                            beforeSend: function (req) {
                                req.setRequestHeader('Authorization', 'Bearer ' + accessToken);
                                req.setRequestHeader('X-App-Id', $rootScope.currentAppId);
                                req.setRequestHeader('X-Organization-Id', $rootScope.currentOrgId);
                            }
                        }
                    },
                    schema: {
                        data: "items",
                        total: "count",
                        model: {
                            id: "id",
                            fields: {
                                Module: { type: "string" },
                                DependencyType: { type: "enums" },
                                ParentField: { type: "string" },
                                ChildField: { type: "string" },
                                ChildSection: { type: "string" },
                            }
                        }
                    }

                },
                scrollable: false,
                persistSelection: true,
                sortable: true,
                filterable: {
                    extra: false
                },
                filter: function (e) {
                    if (e.filter && e.field !== 'DependencyType') {
                        for (var i = 0; i < e.filter.filters.length; i++) {
                            e.filter.filters[i].ignoreCase = true;
                        }
                    }
                },
                rowTemplate: function (e) {
                    var trTemp = '<tr ng-click="goUrl(dataItem)">';
                    trTemp += '<td class="text-left"><span>' + e.module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += e.dependency_type === 'display' ? '<td><span>' + $filter('translate')('Setup.Modules.DependencyTypeDisplay') + '</span></td>' :
                        e.dependency_type === 'freeze' ? '<td><span>' + $filter('translate')('Setup.Modules.DependencyTypeFreeze') + '</span></td>' : '<td><span>' + $filter('translate')('Setup.Modules.DependencyTypeValueChange') + '</span></td>';
                    trTemp += '<td class="text-capitalize left"> <span>' + $filter('filter')(e.module.fields, { name: e.parent_field }, true)[0]['label_' + $scope.language] + '</span></td > ';
                    trTemp += e.child_field ? '<td class="text-capitalize"> <span>' + $filter('filter')(e.module.fields, { name: e.child_field }, true)[0]['label_' + $scope.language] + '</span></td > ' : '<td><span>-</span></td>';
                    trTemp += e.child_section ? '<td class="text-capitalize"> <span>' + $filter('filter')(e.module.sections, { name: e.child_section }, true)[0]['label_' + $scope.language] + '</span></td > ' : '<td><span>-</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';

                    return trTemp;
                },
                altRowTemplate: function (e) {
                    var trTemp = '<tr class="k-alt" ng-click="goUrl(dataItem)">';
                    trTemp += '<td class="text-left"><span>' + e.module['label_' + $scope.language + '_plural'] + '</span></td>';
                    trTemp += e.dependency_type === 'display' ? '<td><span>' + $filter('translate')('Setup.Modules.DependencyTypeDisplay') + '</span></td>' :
                        e.dependency_type === 'freeze' ? '<td><span>' + $filter('translate')('Setup.Modules.DependencyTypeFreeze') + '</span></td>' : '<td><span>' + $filter('translate')('Setup.Modules.DependencyTypeValueChange') + '</span></td>';
                    trTemp += '<td class="text-capitalize"> <span>' + $filter('filter')(e.module.fields, { name: e.parent_field }, true)[0]['label_' + $scope.language] + '</span></td > ';
                    trTemp += e.child_field ? '<td class="text-capitalize"> <span>' + $filter('filter')(e.module.fields, { name: e.child_field }, true)[0]['label_' + $scope.language] + '</span></td > ' : '<td><span>-</span></td>';
                    trTemp += e.child_section ? '<td class="text-capitalize"> <span>' + $filter('filter')(e.module.sections, { name: e.child_section }, true)[0]['label_' + $scope.language] + '</span></td > ' : '<td><span>-</span></td>';
                    trTemp += '<td ng-click="$event.stopPropagation();"> <button ng-click="$event.stopPropagation(); delete(dataItem, $event);" type="button" class="action-button2-delete"><i class="fas fa-trash"></i></button></td></tr>';

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
                        field: 'Module.LabelEnPlural',
                        title: $filter('translate')('Setup.Templates.Module'),
                        headerAttributes: {
                            'class': 'text-left'
                        },
                    },
                    {
                        field: 'DependencyType',
                        title: $filter('translate')('Setup.Modules.DependencyType'),
                        values: [
                            { text: 'Display', value: 'Display' },
                            { text: 'List Text', value: 'ListText' },
                            { text: 'List Value', value: 'ListValue' },
                            { text: 'List Field', value: 'ListField' },
                            { text: 'Lookup Text', value: 'LookupText' },
                            { text: 'Lookup List', value: 'LookupList' },
                            { text: 'Freeze', value: 'Freeze' },
                            { text: 'Lookup Field', value: 'LookupField' }
                        ]
                    },
                    {
                        field: 'ParentField',
                        title: $filter('translate')('Setup.Modules.DependencyParentField'),
                    },
                    {
                        field: 'ChildField',
                        title: $filter('translate')('Setup.Modules.DependencyChildField'),
                    },
                    {
                        field: 'ChildSection',
                        title: $filter('translate')('Setup.Modules.DependencyChildSection'),
                    },
                    {
                        field: '',
                        title: '',
                        width: "90px"
                    }]
            };

            //For Kendo UI
        }
    ]);
