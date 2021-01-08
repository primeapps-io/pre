'use strict';
angular.module('primeapps').controller('RecordDetailController', ['$rootScope', '$scope',
    function ($rootScope, $scope) {
        $rootScope.breadcrumblist = [
            {
                title: "Dashboard",
                link: "link gelecek"
            },
            {
                title: "Module Name",
                link: "link gelecek"
            },    
            {
                title: "Record Name"
            }
        ];

        $("#ticketsForm, #ticketsForm2").kendoValidator().data("kendoValidator");
        var data = [
            "12 Angry Men",
            "Il buono, il brutto, il cattivo.",
            "Inception",
            "One Flew Over the Cuckoo's Nest",
            "Pulp Fiction",
            "Schindler's List",
            "The Dark Knight",
            "The Godfather",
            "The Godfather: Part II",
            "The Shawshank Redemption"
        ];

        $("#search, #search2").kendoAutoComplete({
            dataSource: data,
            separator: ", "
        });

        $("#time, #time2").kendoDropDownList({
            optionLabel: "--Start time--"
        });


        $("#amount, #amount2").kendoNumericTextBox();

        $("#datepicker, #datepicker2").kendoDatePicker();
        $("#grid, #grid2, #grid3").kendoGrid({
            dataSource: {
                pageSize: 40,
                transport: {
                    read: {
                        url: "https://demos.telerik.com/kendo-ui/service/Products",
                        dataType: "jsonp"
                    }
                },
                schema: {
                    model: {
                        id: "ProductID"
                    }
                }
            },
            pageable: true,
            scrollable: false,
            persistSelection: true,
            sortable: true,
            columns: [
                {selectable: true, width: "50px"},
                {field: "ProductName", title: "Product Name"},
                {field: "UnitPrice", title: "Unit Price", format: "{0:c}"},
                {field: "UnitsInStock", title: "Units In Stock"},
                {field: "Discontinued"}]
        });
    }
]);