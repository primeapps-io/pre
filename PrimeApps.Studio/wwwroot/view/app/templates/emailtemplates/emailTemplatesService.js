angular.module('primeapps')

    .factory('EmailTemplatesService', ['$rootScope', '$http', 'config', '$q', '$filter','ModuleService',
        function ($rootScope, $http, config, $q, $filter,ModuleService) {
            return {

                get: function (id) {
                    return $http.get(config.apiUrl + 'template/get/' + id);
                },

                getAll: function (templateType) {
                    return $http.get(config.apiUrl + 'template/get_all?TemplateType=' + templateType);
                },

                create: function (quoteTemplate) {
                    return $http.post(config.apiUrl + 'template/create', quoteTemplate);
                },
                update: function (quoteTemplate) {
                    return $http.put(config.apiUrl + 'template/update/' + quoteTemplate.id, quoteTemplate);
                },

                delete: function (id) {
                    return $http.delete(config.apiUrl + 'template/delete/' + id);
                },
                count: function (templateType) {
                    return $http.get(config.apiUrl + 'template/count?TemplateType=' + templateType);
                },
                find: function (data, templateType) {
                    return $http.post(config.apiUrl + 'template/find?TemplateType=' + templateType, data);
                },
                getFields: function (module) {
                    var moduleFields = angular.copy(module.fields);
                    var fields = [];
                    moduleFields = $filter('filter')(moduleFields, { display_list: true, lookup_type: '!relation' }, true);

                    var seperatorFieldMain = {};
                    seperatorFieldMain.name = 'seperator-main';
                    seperatorFieldMain.label = $rootScope.language === 'tr' ? module.label_tr_singular : module.label_en_singular;
                    seperatorFieldMain.order = 0;
                    seperatorFieldMain.seperator = true;
                    moduleFields.push(seperatorFieldMain);
                    var seperatorLookupOrder = 0;

                    angular.forEach(moduleFields, function (field) {
                        if (field.data_type === 'lookup' && field.lookup_type != 'relation') {
                            var lookupModule = angular.copy($filter('filter')($rootScope.appModules, { name: field.lookup_type }, true)[0]);
                            seperatorLookupOrder += 100;
                            if (lookupModule === null || lookupModule === undefined) return;
                            var seperatorFieldLookup = {};
                            seperatorFieldLookup.name = 'seperator-' + lookupModule.name;
                            seperatorFieldLookup.order = seperatorLookupOrder;
                            seperatorFieldLookup.seperator = true;

                            if ($rootScope.language === 'tr')
                                seperatorFieldLookup.label = lookupModule.label_tr_singular + ' (' + field.label_tr + ')';
                            else
                                seperatorFieldLookup.label = lookupModule.label_en_singular + ' (' + field.label_en + ')';

                            moduleFields.push(seperatorFieldLookup);

                            var lookupModuleFields = angular.copy(lookupModule.fields);
                            lookupModuleFields = $filter('filter')(lookupModuleFields, { display_list: true }, true);

                            angular.forEach(lookupModuleFields, function (fieldLookup) {
                                if (fieldLookup.data_type === 'lookup')
                                    return;

                                fieldLookup.label = $rootScope.language === 'tr' ? fieldLookup.label_tr : fieldLookup.label_en;
                                fieldLookup.labelExt = '(' + field.label + ')';
                                fieldLookup.name = field.name + '.' + fieldLookup.name;
                                fieldLookup.order = parseInt(fieldLookup.order) + seperatorLookupOrder;
                                fieldLookup.parent_type = field.lookup_type;
                                moduleFields.push(fieldLookup);
                            });
                        }
                    });

                    angular.forEach(moduleFields, function (field) {
                        // if (field.deleted || !ModuleService.hasFieldDisplayPermission(field))
                        //     return;

                        if (field.name && field.data_type != 'lookup') {
                            var newField = {};
                            newField.name = field.name;
                            newField.label = $rootScope.language === 'tr' ? field.label_tr : field.label_en;
                            newField.labelExt = field.labelExt;
                            newField.order = field.order;
                            newField.lookup_type = field.lookup_type;
                            newField.seperator = field.seperator;
                            newField.multiline_type = field.multiline_type;
                            newField.data_type = field.data_type;
                            newField.parent_type = field.parent_type;
                            fields.push(newField);
                        }

                    });

                    fields = $filter('orderBy')(fields, 'order');

                    return fields;
                }
            };
        }]);