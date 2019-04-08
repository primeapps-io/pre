'use strict';

angular.module('primeapps')

    .controller('BrandingController', ['$rootScope', '$scope', '$filter', '$state', '$stateParams', '$modal', '$timeout', 'helper', 'dragularService', 'BrandingService', 'LayoutService', '$http', 'config', '$location', 'FileUploader', '$cookies', '$localStorage', '$q', 'resizeService',
        function ($rootScope, $scope, $filter, $state, $stateParams, $modal, $timeout, helper, dragularService, BrandingService, LayoutService, $http, config, $location, FileUploader, $cookies, $localStorage, $q, resizeService) {

            $scope.appModel = {};
            $scope.authTheme = {};
            $scope.appTheme = {};
            $scope.image = {};
            $scope.imageTotal = {};
            $scope.imageRun = {};
            $scope.$parent.activeMenuItem = 'branding';
            $rootScope.breadcrumblist[2].title = 'Branding';
            $scope.tabManage = {
                activeTab: "loginpage"
            };

            $scope.uploaderImage = function (field) {
                $scope.image[field] = {};
                var uploader = $scope.uploaderImage[field] = new FileUploader({
                    url: 'storage/upload_logo',
                    headers: {
                        'Authorization': 'Bearer ' + window.localStorage.getItem('access_token'),//$localStorage.get('access_token'),
                        'Accept': 'application/json',
                        'X-Organization-Id': $rootScope.currentOrgId,
                        'X-App-Id': $scope.appId
                    },
                    queueLimit: 1
                });

                uploader.onAfterAddingFile = function (item) {
                    readFile(item._file)
                        .then(function (image) {
                            item.image = image;
                            var img = new Image();
                            resizeService.resizeImage(item.image, { width: 1024 }, function (err, resizedImage) {
                                if (err)
                                    return;

                                item.file.size = item._file.size;
                                $scope.fileLoadingCounter++;
                                var selectedFile = item.uploader.queue[0].file.name;
                                $scope.imageTotal[field] = selectedFile;
                                $scope.image[field]['Name'] = item.uploader.queue[0].file.name;
                                $scope.image[field]['Size'] = item.uploader.queue[0].file.size;
                                $scope.image[field]['Type'] = item.uploader.queue[0].file.type;

                                $scope.imageRun[field] = item.uploader.queue[0];
                                var images = $scope.imageRun;

                                if (field === 'authBanner') {
                                    $scope.authBanner = item.uploader.queue[0];
                                    var authBanner = images['authBanner'];
                                    authBanner.upload();
                                }

                                if (field === 'authLogo') {
                                    $scope.authLogo = item.uploader.queue[0];
                                    var authLogo = images['authLogo'];
                                    authLogo.upload();
                                }

                                if (field === 'authFavicon') {
                                    $scope.authFavicon = item.uploader.queue[0];
                                    var authFavicon = images['authFavicon'];
                                    authFavicon.upload();
                                }

                                if (field === 'appThemeLogo') {
                                    $scope.appThemeLogo = item.uploader.queue[0];
                                    var appThemeLogo = images['appThemeLogo'];
                                    appThemeLogo.upload();
                                }

                                if (field === 'appThemeFavicon') {
                                    $scope.appThemeFavicon = item.uploader.queue[0];
                                    var appThemeFavicon = images['appThemeFavicon'];
                                    appThemeFavicon.upload();
                                }

                                if (authLogo) {
                                    authLogo.uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                                        if (status === 200) {
                                            $scope.authTheme.logo = logoUrl;
                                        }
                                    };
                                }

                                if (authBanner) {
                                    authBanner.uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                                        if (status === 200) {
                                            $scope.authTheme.banner = logoUrl;
                                        }
                                    };
                                }

                                if (authFavicon) {
                                    authFavicon.uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                                        if (status === 200) {
                                            $scope.authTheme.favicon = logoUrl;
                                        }
                                    };
                                }
                                if (appThemeLogo) {
                                    appThemeLogo.uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                                        if (status === 200) {
                                            $scope.appTheme.logo = logoUrl;
                                        }
                                    };
                                }

                                if (appThemeFavicon) {
                                    appThemeFavicon.uploader.onCompleteItem = function (fileItem, logoUrl, status) {
                                        if (status === 200) {
                                            $scope.appTheme.favicon = logoUrl;
                                        }
                                    };
                                }
                            });
                        });
                };
                uploader.onWhenAddingFileFailed = function (item, filter, options) {
                    switch (filter.name) {
                        case 'imgFilter':
                            ngToast.create({
                                content: $filter('translate')('Setup.Settings.ImageError'),
                                className: 'warning'
                            });
                            break;
                        case 'sizeFilter':
                            ngToast.create({
                                content: $filter('translate')('Setup.Settings.SizeError'),
                                className: 'warning'
                            });
                            break;
                    }
                };

                uploader.filters.push({
                    name: 'imgFilter',
                    fn: function (item) {
                        var extension = helper.getFileExtension(item.name);
                        return true ? (extension === 'jpg' || extension == 'jpeg' || extension == 'png' || extension == 'doc' || extension == 'gif' || extension == 'ico') : false;
                    }
                });

                uploader.filters.push({
                    name: 'sizeFilter',
                    fn: function (item) {
                        return item.size < 5242880;// 5mb
                    }
                });

                // uploader_image.onSuccessItem = function (item, response) {
                //     $scope.image[field.name]['UniqueName'] = response.UniqueName;
                //     $scope.fileLoadingCounter--;
                // };

                function readFile(file) {
                    var deferred = $q.defer();
                    var reader = new FileReader();

                    reader.onload = function (e) {
                        deferred.resolve(e.target.result);
                    };

                    reader.readAsDataURL(file);

                    return deferred.promise;
                }

                return uploader;

            };

            // $scope.logoRemove = function () {
            //     if (uploader.queue[0]) {
            //         //uploader.queue[0].image = null;
            //         uploader.queue[0].remove();
            //     }
            // };

            BrandingService.getAppTheme($scope.appId).then(function (response) {
                var appTheme = response.data;
                $scope.appTheme.color = appTheme.color;
                $scope.appTheme.title = appTheme.title;
                $scope.appTheme.favicon = appTheme.favicon;
                $scope.appTheme.logo = appTheme.logo;
            });

            $scope.saveAppTheme = function () {
                $scope.savingApp = true;
                var appThemes = {};
                appThemes.color = $scope.appTheme.color;
                appThemes.title = $scope.appTheme.title;
                appThemes.favicon = $scope.appTheme.favicon;
                appThemes.logo = $scope.appTheme.logo;

                BrandingService.updateAppTheme($scope.appId, appThemes)
                    .then(function (response) {
                        toastr.success($filter('translate')('Branding is updated successfully.'));
                        $scope.savingApp = false;
                    });
            };

            BrandingService.getAuthTheme($scope.appId).then(function (response) {
                var authTheme = response.data;
                if (authTheme && authTheme.banner) {
                    $scope.authTheme.banner = authTheme.banner[0].image;
                    $scope.authTheme.descriptionTr = authTheme.banner[0].description.tr;
                    $scope.authTheme.descriptionEn = authTheme.banner[0].description.en;
                }
                $scope.authTheme.color = authTheme.color;
                $scope.authTheme.title = authTheme.title;
                $scope.authTheme.favicon = authTheme.favicon;
                $scope.authTheme.logo = authTheme.logo;
            });


            $scope.saveAuthTheme = function () {
                $scope.savingAuth = true;
                var authThemes = {};
                var description = {};
                description.en = $scope.authTheme.descriptionEn;
                description.tr = $scope.authTheme.descriptionTr;
                var banner = [
                    { descriptions: description, image: $scope.authTheme.banner }
                ];
                authThemes.color = $scope.authTheme.color;
                authThemes.title = $scope.authTheme.title;
                authThemes.banner = banner;
                authThemes.favicon = $scope.authTheme.favicon;
                authThemes.logo = $scope.authTheme.logo;
                BrandingService.updateAuthTheme($scope.appId, authThemes)
                    .then(function (response) {
                        toastr.success($filter('translate')('Branding is updated successfully.'));
                        $scope.savingAuth = false;
                    });
            };

            // $scope.addMasterUser = function () {
            //     var newCol = {};
            //     newCol.role_id = 1;
            //     newCol.profile_id = 1;
            //     newCol.first_name = "master";
            //     newCol.last_name = "test";
            //     newCol.email = "master.test@usertest3.com";
            //     newCol.password = "1234567";
            //     newCol.created_at = new Date();
            //
            //     BrandingService.addAppUser(newCol)
            //         .then(function (response) {
            //             if (response.data) {
            //                 toastr.success('Collaborator is saved successfully');
            //
            //             }
            //         })
            //         .catch(function () {
            //             toastr.error($filter('translate')('Common.Error'));
            //
            //         });
            // };
        }
    ]);