"use strict";angular.module("primeapps").factory("SignalNotificationService",["$http","config",function(i,n){return{getAll:function(t){return i.get(t?n.apiUrl+"signal_notification/get_unreads/"+t:n.apiUrl+"signal_notification/get_unreads")},read:function(t){return i.put(n.apiUrl+"signal_notification/read/"+t)},hide:function(t){return i.put(n.apiUrl+"signal_notification/hide/"+t)}}}]);