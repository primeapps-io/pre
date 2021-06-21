"use strict";angular.module("images-resizer",[]);angular.module("images-resizer").service("resizeService",["$q",function($q){var mainCanvas,_this=this,isCanvasSupported=!(!document.createElement("canvas").getContext||!document.createElement("canvas").getContext("2d"));this.createImage=function(src){var deferred=$q.defer(),img=new Image;return img.onload=function(){deferred.resolve(img)},img.src=src,deferred.promise},this.resizeCanvas=function(cnv,width,height){if(!width||!height)return cnv;var tmpCanvas=document.createElement("canvas");tmpCanvas.width=width,tmpCanvas.height=height;var cnx=tmpCanvas.getContext("2d");cnx.fillStyle = "white";cnx.fillRect(0,0,tmpCanvas.width, tmpCanvas.height);return cnx.drawImage(cnv,0,0,tmpCanvas.width,tmpCanvas.height),tmpCanvas},this.resizeImage=function(src,options,cb){return isCanvasSupported?cb&&options&&src?(options={height:options.height?options.height:options.width?null:options.size?null:1024,width:options.width?options.width:options.height?null:options.size?null:1024,size:options.size?options.size:500,sizeScale:options.sizeScale?options.sizeScale:"ko",step:options.step?options.step:3},void _this.createImage(src).then(function(img){if(options.height||options.width)_this.createImage(src).then(function(img){cb(null,_this.resizeImageWidthHeight(img,options.width,options.height,options.step))},function(err){cb(err,null)});else if(options.size){if("string"==typeof options.sizeScale)switch(options.sizeScale.toLowerCase()){case"ko":options.size*=1024;break;case"mo":options.size*=1048576;break;case"go":options.size*=1073741824}cb(null,_this.resizeImageBySize(img,options.size,options.step))}},function(err){cb(err,null)})):void cb("Missing argument when calling resizeImage function",null):void cb("Canvas is not supported on your browser",null)},this.resizeImageWidthHeight=function(image,width,height,step){if(!image)return null;mainCanvas=document.createElement("canvas"),width||height?!width&&height?width=height/image.height*image.width:width&&!height&&(height=width/image.width*image.height):(width=image.width,height=image.height);var pixelStepWidth=image.width!==width&&step?(image.width-width)/step:0,pixelStepHeight=image.height!==height&&step?(image.height-height)/step:0;mainCanvas.width=image.width,mainCanvas.height=image.height,mainCanvas.getContext("2d").drawImage(image,0,0,mainCanvas.width,mainCanvas.height);for(var i=1;step>i;i++){var newWidth=image.width-pixelStepWidth*i,newHeight=image.height-pixelStepHeight*i;mainCanvas=this.resizeCanvas(mainCanvas,newWidth,newHeight)}return mainCanvas=this.resizeCanvas(mainCanvas,width,height),mainCanvas.toDataURL("image/jpeg")},this.resizeImageBySize=function(image,size){if(!image)return null;mainCanvas=document.createElement("canvas"),mainCanvas.width=image.width,mainCanvas.height=image.height,mainCanvas.getContext("2d").drawImage(image,0,0,mainCanvas.width,mainCanvas.height);for(var tmpResult=mainCanvas.toDataURL("image/jpeg"),result=tmpResult,sizeOfTheImage=3*Math.round(tmpResult.length-"data:image/jpg;base64,".length)/4,divideStrategy=1>=sizeOfTheImage/(2*size)?.9:sizeOfTheImage/(2.3*size);sizeOfTheImage>size;){var canvas=document.createElement("canvas");canvas.width=mainCanvas.width/divideStrategy,canvas.height=mainCanvas.height/divideStrategy,canvas.getContext("2d").drawImage(mainCanvas,0,0,canvas.width,canvas.height),tmpResult=mainCanvas.toDataURL("image/jpeg");var sizeOfTheImageTmp=3*Math.round(tmpResult.length-"data:image/jpg;base64,".length)/4;.5>sizeOfTheImageTmp/size?divideStrategy=1>=sizeOfTheImage/(2*size)?.9:sizeOfTheImage/(2.3*size):(mainCanvas=canvas,result=tmpResult,sizeOfTheImage=3*Math.round(tmpResult.length-"data:image/jpg;base64,".length)/4),mainCanvas=canvas}return result}}]),angular.module("images-resizer").service("readLocalPicService",["$q",function($q){function eventErrorDecoder(event){var errorMessage=null;switch(event.target.error.code){case FileError.NOT_FOUND_ERR:errorMessage="NOT_FOUND_ERR";break;case FileError.SECURITY_ERR:errorMessage="SECURITY_ERR";break;case FileError.ABORT_ERR:errorMessage="ABORT_ERR";break;case FileError.NOT_READABLE_ERR:errorMessage="NOT_READABLE_ERR";break;case FileError.ENCODING_ERR:errorMessage="ENCODING_ERR";break;case FileError.NO_MODIFICATION_ALLOWED_ERR:errorMessage="NO_MODIFICATION_ALLOWED_ERR";break;case FileError.INVALID_STATE_ERR:errorMessage="INVALID_STATE_ERR";break;case FileError.SYNTAX_ERR:errorMessage="SYNTAX_ERR";break;case FileError.INVALID_MODIFICATION_ERR:errorMessage="INVALID_MODIFICATION_ERR";break;case FileError.QUOTA_EXCEEDED_ERR:errorMessage="QUOTA_EXCEEDED_ERR";break;case FileError.TYPE_MISMATCH_ERR:errorMessage="TYPE_MISMATCH_ERR";break;case FileError.PATH_EXISTS_ERR:errorMessage="PATH_EXISTS_ERR";break;default:errorMessage="Unknown Error: "+event.target.error.code}return errorMessage}this.readFileInput=function(input){var deferred=$q.defer();if(input.files&&input.files[0]){window.File&&window.FileReader&&window.FileList&&window.Blob||deferred.reject("Your browser do not support reading file");var reader=new FileReader;reader.onload=function(e){deferred.resolve(e.target.result)},reader.onabort=function(e){deferred.reject("Fail to convert file in base64img, aborded: "+eventErrorDecoder(e))},reader.onerror=function(e){deferred.reject("Fail to convert file in base64img, error: "+eventErrorDecoder(e))},reader.readAsDataURL(input.files[0])}else deferred.reject("No file selected");return deferred.promise}}]);