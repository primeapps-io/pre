'use strict';

angular.module('primeapps')

    .controller('AppCollaboratorsController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'AppCollaboratorsService', 'LayoutService', '$http', 'config',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, AppCollaboratorsService, LayoutService, $http, config) {
            //$rootScope.modules = $http.get(config.apiUrl + 'module/get_all');

            //$scope.$parent.menuTopTitle = "Settings";
            //$scope.$parent.activeMenu = 'settings';
            $scope.$parent.activeMenuItem = 'appCollaborators';
            $rootScope.breadcrumblist[2].title = 'App Collaborators';

            $scope.generator = function (limit) {
                $scope.placeholderArray = [];
                for (var i = 0; i < limit; i++) {
                    $scope.placeholderArray[i] = i;
                }

            };
            $scope.generator(10);

            $scope.getTeamsAndCollaborators = function () {
                $scope.collaboratorsAndTeamsArr = [];
                $scope.loadingMembers = true;
                AppCollaboratorsService.getCollaborators($scope.$parent.appId).then(function (response) {
                    $scope.appCollaborators = response.data;

                    var collaboratorArr = [];
                    var teamArr = [];

                    for (var j = 0; j < response.data.length; j++) {
                        var data = response.data[j];

                        if (data.user_id)
                            collaboratorArr.push(data)

                        if (data.team_id)
                            teamArr.push(data)
                    }

                    var organization = $filter('filter')($rootScope.organizations, { id: $rootScope.currentOrganization.id }, true)[0];

                    AppCollaboratorsService.getTeamsByOrganizationId($scope.$parent.orgId)
                        .then(function (response) {
                            if (response.data.length > 0) {

                                var teams = response.data;

                                for (var i = 0; i < teams.length; i++) {

                                    var isExistTeam = false;
                                    if (teamArr.length > 0) {
                                        if ($filter('filter')(teamArr, { team_id: teams[i].id }, true).length > 0)
                                            isExistTeam = true;
                                    }

                                    if (!isExistTeam) {
                                        var teamObj = {};
                                        teamObj.full_name = teams[i].name;
                                        teamObj.id = teams[i].id;
                                        teamObj.type = 'team';
                                        $scope.collaboratorsAndTeamsArr.push(teamObj);
                                    }
                                }
                            }

                            AppCollaboratorsService.getUsersByOrganizationId($scope.$parent.orgId)
                                .then(function (response) {
                                    if (response.data.users.length > 0) {
                                        var collabs = response.data.users;

                                        for (var i = 0; i < collabs.length; i++) {
                                            var isExistCol = false;
                                            if (collaboratorArr.length > 0) {
                                                if ($filter('filter')(collaboratorArr, { user_id: collabs[i].user_id }, true).length > 0)
                                                    isExistCol = true;
                                            }

                                            if (!isExistCol) {
                                                var colObj = {};
                                                colObj.full_name = collabs[i].full_name;
                                                colObj.id = collabs[i].user_id;
                                                colObj.type = 'user';
                                                $scope.collaboratorsAndTeamsArr.push(colObj);
                                            }
                                        }
                                    }

                                    if ($scope.appCollaborators.length > 0) {
                                        for (var j = 0; j < $scope.appCollaborators.length; j++) {
                                            var appCol = $scope.appCollaborators[j];

                                            if (appCol.user_id) {
                                                appCol.name = $filter('filter')(collabs, { user_id: appCol.user_id }, true)[0].full_name;
                                            }

                                            if (appCol.team_id) {
                                                appCol.name = $filter('filter')(teams, { id: appCol.team_id }, true)[0].name;
                                            }
                                        }
                                    }
                                    $scope.loadingMembers = false;
                                })
                                .catch(function (error) {
                                    toastr.error($filter('translate')('Common.Error'));
                                    $scope.loadingMembers = false;
                                });
                        })
                        .catch(function (error) {
                            toastr.error($filter('translate')('Common.Error'));
                            $scope.loadingMembers = false;
                        });
                });
            }

            $scope.getTeamsAndCollaborators();

            $scope.selectItemForApp = function (item) {
                if (!item)
                    return;

                $scope.loadingMembers = true;
                var appCollaboratorObj = {};
                appCollaboratorObj.app_id = $scope.$parent.appId;
                appCollaboratorObj.profile_id = 1;

                if (item.type == 'user') {
                    appCollaboratorObj.user_id = item.id;
                } else {
                    appCollaboratorObj.team_id = item.id;
                }

                AppCollaboratorsService.addAppCollaborator(appCollaboratorObj)
                    .then(function (response) {
                        if (response.data) {
                            if (item.type == 'user') {
                                toastr.success('Collaborator is added successfully');
                            } else {
                                toastr.success('Team is added successfully');
                            }

                            $scope.getTeamsAndCollaborators();
                            $scope.selectedUser = {};

                        }
                    })
                    .catch(function (error) {
                        toastr.error($filter('translate')('Common.Error'));
                    });

            }

            $scope.delete = function (id) {
                swal({
                    title: "Are you sure?",
                    text: " ",
                    icon: "warning",
                    buttons: ['Cancel', 'Yes'],
                    dangerMode: true
                }).then(function (value) {
                    if (value) {
                        if (!id)
                            return false;

                        AppCollaboratorsService.delete(id)
                            .then(function (response) {
                                if (response.data) {
                                    toastr.success("Team is deleted successfully.", "Deleted!");
                                    $scope.getTeamsAndCollaborators();
                                }
                            })
                            .catch(function (result) {
                                toastr.error($filter('translate')('Common.Error'));
                            });
                    }
                });
            };
        }
    ]);