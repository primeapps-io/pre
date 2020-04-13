"use strict";angular.module("primeapps",["ngAnimate","ui.router","oc.lazyLoad","ngCookies","pascalprecht.translate","tmh.dynamicLocale","ui.utils","ngTable","angularFileUpload","blockUI","ngImgCrop","images-resizer","angular-plupload","angucomplete-alt","ui.tinymce","ui.mask","ui.ace","ngSanitize","ngclipboard","mentio","mwl.calendar","angular.filter","kendo.directives","ngMaterial"]).config(["$locationProvider","$compileProvider","$filterProvider","$controllerProvider","$provide","$httpProvider","$qProvider","$sceDelegateProvider","$translateProvider","tmhDynamicLocaleProvider","blockUIConfig","$animateProvider","pluploadOptionProvider","config","$mdThemingProvider",function(e,a,t,r,o,l,n,i,c,f,s,d,u,g,p){function m(e){var a=angular.bind(e,e.fromUrl);return e.fromUrl=function(e,t){if(null!==e&&angular.isDefined(e)&&("function"==typeof e&&(e=e.call(e,t)),angular.isString(e)&&routeTemplateUrls&&routeTemplateUrls.length>0))for(var r=0;r<routeTemplateUrls.length;r++)e.indexOf(routeTemplateUrls[r])>-1&&(e+=e.indexOf("?")<0?"?":"&",e+="v="+(new Date).getTime()/1e3);return a(e,t)},e}angular.module("primeapps").controller=r.register,angular.module("primeapps").service=o.service,angular.module("primeapps").factory=o.factory,angular.module("primeapps").directive=window.origin.contains("localhost")?a.directive:a.debugInfoEnabled(!1),angular.module("primeapps").filter=t.register,angular.module("primeapps").value=o.value,angular.module("primeapps").constant=o.constant,angular.module("primeapps").provider=o.provider,p.definePalette("red",{50:"fbe6e5",100:"f5c1bd",200:"ee9891",300:"e76e65",400:"e14f44",500:"dc3023",600:"d82b1f",700:"d3241a",800:"ce1e15",900:"c5130c",A100:"fff1f1",A200:"ffbfbe",A400:"ff8e8b",A700:"ff7571",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.definePalette("purple",{50:"f1e9f5",100:"ddc7e6",200:"c7a2d6",300:"b07cc6",400:"9f60b9",500:"8e44ad",600:"863ea6",700:"7b359c",800:"712d93",900:"5f1f83",A100:"e7bfff",A200:"d38cff",A400:"bf59ff",A700:"b640ff",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.definePalette("blue",{50:"e4e9f1",100:"bcc8db",200:"8fa3c4",300:"627eac",400:"41629a",500:"1f4688",600:"1b3f80",700:"173775",800:"122f6b",900:"0a2058",A100:"8ca7ff",A200:"5980ff",A400:"2658ff",A700:"0d45ff",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.definePalette("light-blue",{50:"e3f6ff",100:"bae9ff",200:"8cdaff",300:"5ecbfe",400:"3cc0fe",500:"19b5fe",600:"16aefe",700:"12a5fe",800:"0e9dfe",900:"088dfd",A100:"ffffff",A200:"f2f9ff",A400:"bfdfff",A700:"a6d3ff",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.definePalette("green",{50:"e5f4eb",100:"bee4ce",200:"92d3ad",300:"66c18c",400:"46b374",500:"25a65b",600:"219e53",700:"1b9549",800:"168b40",900:"0d7b2f",A100:"adffc2",A200:"7aff9d",A400:"47ff77",A700:"2dff64",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.definePalette("orange",{50:"fff3e0",100:"ffe2b3",200:"ffce80",300:"ffba4d",400:"ffac26",500:"ff9d00",600:"ff9500",700:"ff8b00",800:"ff8100",900:"ff6f00",A100:"ffffff",A200:"fff7f2",A400:"ffd7bf",A700:"ffc7a6",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.definePalette("gray",{50:"eaebed",100:"caced1",200:"a6aeb3",300:"828d94",400:"68747d",500:"4d5c66",600:"46545e",700:"3d4a53",800:"344149",900:"253038",A100:"82ccff",A200:"4fb7ff",A400:"1ca2ff",A700:"0397ff",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.definePalette("secondarypalet",{50:"e0e0e0",100:"b3b3b3",200:"808080",300:"4d4d4d",400:"262626",500:"000000",600:"000000",700:"000000",800:"000000",900:"000000",A100:"a6a6a6",A200:"8c8c8c",A400:"737373",A700:"666666",contrastDefaultColor:"light",contrastDarkColors:["50","100","200","A100","A200"],contrastLightColors:["300","400","500","600","700","800","900","A400","A700"]}),p.theme("red").primaryPalette("red").accentPalette("secondarypalet"),p.theme("purple").primaryPalette("purple").accentPalette("secondarypalet"),p.theme("blue").primaryPalette("blue").accentPalette("secondarypalet"),p.theme("light-blue").primaryPalette("light-blue").accentPalette("secondarypalet"),p.theme("green").primaryPalette("green").accentPalette("secondarypalet"),p.theme("orange").primaryPalette("orange").accentPalette("secondarypalet"),p.theme("gray").primaryPalette("gray").accentPalette("secondarypalet"),p.theme("toast-success"),p.theme("toast-error"),p.theme("toast-warning"),p.theme("toast-info"),p.alwaysWatchTheme(!0),e.hashPrefix("");var A=[];blobUrl&&A.push(blobUrl+"**"),angular.forEach(trustedUrls,function(e){A.push(e.url+"**")}),A.length>0&&(A.push("self"),i.resourceUrlWhitelist(A)),l.interceptors.push("genericInterceptor");var h=account.user.setting.language;if(!h&&customLanguage)h=customLanguage;else if(!h&&!customLanguage){var b=window.navigator.language||window.navigator.userLanguage;h="tr"===b||"tr-TR"===b?"tr":"en"}window.localStorage.setItem("NG_TRANSLATE_LANG_KEY",h),moment.locale(h),c.useStaticFilesLoader({prefix:"locales/",suffix:".json"}).useLocalStorage().preferredLanguage(h).useSanitizeValueStrategy(null);var v=window.localStorage.locale_key||h;f.defaultLocale(v),f.localeLocationPattern("scripts/vendor/locales/angular-locale_{{locale}}.js"),s.autoBlock=!1,s.message="",s.templateUrl="view/common/blockui.html",d.classNameFilter(/^(?:(?!ng-animate-disabled).)*$/),mOxie.Mime.mimes.csv=".csv",u.setOptions({runtimes:"html5",url:"storage/upload",chunk_size:"5mb",multipart:!0,unique_names:!0}),o.decorator("$templateFactory",["$delegate",m])}]).run(["$rootScope","$location","$state","$q","$window","AuthService","AppService","$localStorage","$translate","$cache","helper","$mdSidenav","$cookies",function(e,a,t,r,o,l,n,i,c,f,s,d){e.appTheme=appTheme;var u=s.parseQueryString(o.location.hash.substr(2)),g=u.lang,p=l.isAuthenticated();return e.preview=preview,e.sideLoad=!1,e.buildToggler=function(a,t,r){e.sideinclude=!0,e.url=t,e.mdSidenavScope=r,setTimeout(function(){e.sideLoad=!0},250),setTimeout(function(){d(a).open()},100)},e.buildToggler2=function(e){return function(){d(e).toggle(),angular.element("#wrapper").removeClass("hide-sidebar")}},e.closeSide=function(a){d(a).close(),e.sideinclude=!1,e.sideLoad=!1},d("sideModal",!0).then(function(a){a.onClose(function(){e.sideinclude=!1,e.sideLoad=!1})}),e.sideModaldock=function(){e.isDocked=!e.isDocked,e.isDocked?angular.element("#wrapper").addClass("hide-sidebar"):angular.element("#wrapper").removeClass("hide-sidebar")},!g||"en"!==g&&"tr"!==g||(i.write("NG_TRANSLATE_LANG_KEY",g),c.use(g),e.language=g),p?void e.$on("$stateChangeStart",function(){try{e.currentPath=a.$$url,e.administrationMenuActive=e.currentPath.indexOf("/app/setup/")>-1?!0:!1}catch(t){return}}):void(o.location.href="/")}]);