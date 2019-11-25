"use strict";angular.module("primeapps").controller("SingleEMailController",["$rootScope","$scope","ngToast","$filter","helper","$location","$state","$stateParams","$q","$window","$localStorage","$cache","config","$http","$popover","ModuleService","TemplateService","$cookies",function(e,t,a,n,l,i,s,o,r,c,m,d,u,p,g,f,h,_){function y(e){var a=[];return f.getTemplates(t.type,"module").then(function(n){if(0!=n.data.length){t.quoteTemplates=n.data;for(var l=0;l<t.quoteTemplates.length;l++)a[l]={text:t.quoteTemplates[l].name,id:l,onclick:function(){e.setProgressState(!0),t.addTemplate("attachment",t.quoteTemplates[this.settings.id],e)}}}}),a}t.loadingModal=!0,t.module=n("filter")(e.modules,{name:o.type},!0)[0],t.iframeElement={},t.tinymceOptions=function(a){t[a]={setup:function(e){e.addButton("addParameter",{type:"button",text:n("translate")("EMail.AddParameter"),onclick:function(){tinymce.activeEditor.execCommand("mceInsertContent",!1,"#")}}),e.on("init",function(){t.loadingModal=!1})},onChange:function(){},inline:!1,height:300,language:e.language,plugins:["advlist autolink lists link image charmap print preview anchor table","searchreplace visualblocks code fullscreen","insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"],imagetools_cors_hosts:["crm.ofisim.com","test.ofisim.com","ofisimcomdev.blob.core.windows.net"],toolbar:"addParameter | styleselect | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify | table bullist numlist | link image imagetools |  cut copy paste | undo redo searchreplace | outdent indent | blockquote hr insertdatetime charmap | visualblocks code preview fullscreen",menubar:"false",templates:[{title:"Test template 1",content:"Test 1"},{title:"Test template 2",content:"Test 2"}],skin:"lightgray",theme:"modern",file_picker_callback:function(e,t,a){if(b=e,"file"==a.filetype){var n=document.getElementById("uploadFile");n.click()}if("image"==a.filetype){var n=document.getElementById("uploadImage");n.click()}},image_advtab:!0,file_browser_callback_types:"image file",paste_data_images:!0,paste_as_text:!0,spellchecker_language:e.language,images_upload_handler:function(e,a,n){var l=e.blob();b=a,v=n,t.imgUpload.uploader.addFile(l)},init_instance_callback:function(e){t.iframeElement[a]=e.iframeElement},resize:!1,width:"99,9%",toolbar_items_size:"small",statusbar:!1,convert_urls:!1,remove_script_host:!1}},t.tinymceOptions("tinymceTemplate"),t.tinymceOptions("tinymceTemplateEdit"),t.formType="email",t.templateSave=function(){var e={};e.module=t.module.name,e.name=t.template_name,e.subject=t.template_subject,e.content=t.tinymce_content,e.sharing_type=t.newtemplate.sharing_type,e.template_type=2,e.active=!0,"custom"===t.newtemplate.system_type&&(e.shares=[],angular.forEach(t.newtemplate.shares,function(t){e.shares.push(t.id)}));var l;t.currentTemplate?(e.id=t.currentTemplate.id,l=h.update(e)):l=h.create(e),l.then(function(e){t.currentTemplate=e.data,h.getAll("email",t.module.name).then(function(l){t.templates=l.data,t.template=e.data.id,a.create({content:n("translate")("Template.SuccessMessage"),className:"success"})})})},t.templateDelete=function(){var e;e=t.template,h["delete"](e).then(function(){h.getAll("email",t.module.name).then(function(e){t.templates=e.data}),a.create({content:n("translate")("Template.SuccessDelete"),className:"success"})})},t.setContent=function(e){var a=n("filter")(t.templates,{id:e},!0)[0];e?(t.newtemplate.system_type="custom",t.newtemplate.sharing_type="me",t.tinymceModel=a.content,t.subject=a.subject,t.currentTemplate=a,t.template_name=a.name):(t.tinymceModel=null,t.subject=null,t.currentTemplate=null,t.template_name=null,t.template_subject=null,t.tinymce_content=null,t.newtemplate.system_type=null,t.shares=null)},t.setTemplate=function(){t.template_subject=t.subject,t.tinymce_content=t.tinymceModel,t.currentTemplate?(t.newtemplate.sharing_type=t.currentTemplate.sharing_type,t.newtemplate.shares=t.currentTemplate.shares):t.newtemplate.sharing_type="me"},t.backTemplate=function(){t.subject=t.template_subject,t.tinymceModel=t.tinymce_content},t.newtemplate={},t.newtemplate.system_type="custom",t.newtemplate.sharing_type="me";var b,v,k=angular.copy(e.system.messaging.SystemEMail||{}),w=angular.copy(e.system.messaging.PersonalEMail||{}),$=plupload.guid();t.moduleFields=h.getFields(t.module),t.emailFields=[],angular.forEach(t.moduleFields,function(e){"email"!==e.data_type||e.deleted||"users"==e.parent_type||t.emailFields.push(e)}),t.emailFields.length>0&&(t.emailField=t.emailFields[0]),t.senderAlias=null,t.senders=[],k.senders&&k.senders.length>0&&k.senders.forEach(function(e){e.type="System",t.senders.push(e)}),w.senders&&w.senders.length>0&&w.senders.forEach(function(e){e.type="Personal",t.senders.push(e)}),t.getTagTextRaw=function(e){return e.name.indexOf("seperator")>=0?void 0:'<i style="color:#0f1015;font-style:normal">{'+e.name+"}</i>"},t.searchTags=function(e){var a=[];return angular.forEach(t.moduleFields,function(t){"seperator"!=t.name&&t.label.indexOf(e)>=0&&a.push(t)}),t.tags=a,a},t.primaryField=n("filter")(t.module.fields,{primary:!0})[0],t.recordId=t.$parent.$parent.id,h.getAll("email",t.module.name).then(function(e){t.templates=e.data}),t.addressType=function(e){return n("translate")("EMail."+e)},t.imgUpload={settings:{multi_selection:!1,url:"storage/upload",multipart_params:{container:$,type:"mail",upload_id:0,response_list:""},filters:{mime_types:[{title:"Image files",extensions:"jpg,gif,png"}],max_file_size:"2mb"},resize:{quality:90}},events:{filesAdded:function(e){e.start(),tinymce.activeEditor.windowManager.open({title:n("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+n("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){e.settings.multipart_params.response_list="",e.settings.multipart_params.upload_id=0,tinymce.activeEditor.windowManager.close();var n=JSON.parse(a.response);b(u.storage_host+n.public_url,{alt:t.name}),b=null},error:function(e,t){switch(t.code){case-600:tinymce.activeEditor.windowManager.alert(n("translate")("EMail.MaxImageSizeExceeded"))}v&&(v(),v=null)}}},t.fileUpload={settings:{multi_selection:!1,unique_names:!1,url:u.apiUrl+"Document/upload_attachment",headers:{Authorization:"Bearer "+m.read("access_token"),Accept:"application/json","X-Tenant-Id":_.get("tenant_id")},multipart_params:{container:$},filters:{mime_types:[{title:"Email Attachments",extensions:"pdf,doc,docx,xls,xlsx,csv"}],max_file_size:"50mb"}},events:{filesAdded:function(e){e.start(),tinymce.activeEditor.windowManager.open({title:n("translate")("Common.PleaseWait"),width:50,height:50,body:[{type:"container",name:"container",label:"",html:"<span>"+n("translate")("EMail.UploadingAttachment")+"</span>"}],buttons:[]})},uploadProgress:function(){},fileUploaded:function(e,t,a){var n=JSON.parse(a.response);b(u.storage_host+n.public_url,{alt:t.name}),b=null,tinymce.activeEditor.windowManager.close()},chunkUploaded:function(e,t,a){var n=JSON.parse(a.response);n.upload_id&&(e.settings.multipart_params.upload_id=n.upload_id),e.settings.multipart_params.response_list+=""==e.settings.multipart_params.response_list?n.e_tag:"|"+n.e_tag},error:function(e,t){switch(this.settings.multipart_params.response_list="",this.settings.multipart_params.upload_id=0,t.code){case-600:tinymce.activeEditor.windowManager.alert(n("translate")("EMail.MaxFileSizeExceeded"))}v&&(v(),v=null)}}},t.tinymceOptions={setup:function(a){a.addButton("addQuoteTemplate",{type:"menubutton",text:n("translate")("EMail.AddPdfTemplate",{module:t.module["label_"+e.language+"_singular"]}),icon:!1,menu:y(a)}),a.addButton("addParameter",{type:"button",text:n("translate")("EMail.AddParameter"),onclick:function(){tinymce.activeEditor.execCommand("mceInsertContent",!1,"#")}}),a.on("init",function(){t.loadingModal=!1,t.editor=a})},onChange:function(){},inline:!1,height:300,language:e.language,plugins:["advlist autolink lists link image charmap print preview anchor","searchreplace visualblocks code fullscreen","insertdatetime table contextmenu paste imagetools wordcount textcolor colorpicker"],imagetools_cors_hosts:["crm.ofisim.com","test.ofisim.com","ofisimcomdev.blob.core.windows.net"],toolbar:"addParameter | addQuoteTemplate | styleselect | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify | table bullist numlist | link image imagetools |  cut copy paste | undo redo searchreplace | outdent indent | blockquote hr insertdatetime charmap | visualblocks code preview fullscreen",menubar:"false",templates:[{title:"Test template 1",content:"Test 1"},{title:"Test template 2",content:"Test 2"}],skin:"lightgray",theme:"modern",file_picker_callback:function(e,t,a){if(b=e,"file"==a.filetype){var n=document.getElementById("uploadFile");n.click()}if("image"==a.filetype){var n=document.getElementById("uploadImage");n.click()}},image_advtab:!0,file_browser_callback_types:"image file",paste_data_images:!0,paste_as_text:!0,spellchecker_language:e.language,images_upload_handler:function(e,a,n){var l=e.blob();b=a,v=n,t.imgUpload.uploader.addFile(l)},init_instance_callback:function(e){t.iframeElement=e.iframeElement},resize:!1,width:"99,9%",toolbar_items_size:"small",statusbar:!1,convert_urls:!1,remove_script_host:!1},t.addCustomField=function(e,t){tinymce.activeEditor.execCommand("mceInsertContent",!1,"{"+t.name+"}")},t.addTemplate=function(a,l,i){if(l){t.templateAdding={},t.templateAdding[a]=!0;var s=n("filter")(t.$parent.$parent.module.fields,{primary:!0},!0)[0],o=t.$parent.$parent.record[s.name]+".pdf";if(l.link)"link"===a?tinymce.activeEditor.execCommand("mceInsertContent",!1,'<a href="'+l.link.fileurl+'">'+o+"</a>"):(t.attachmentLink=l.link.fileurl,t.attachmentName=o.substring(0,50),t.quoteTemplateName=" ( "+l.name+" ) "),t.templateAdding[a]=!1,i.setProgressState(!1),t.$apply();else{var r=u.apiUrl+"Document/export?module="+t.type+"&id="+t.$parent.$parent.record.id+"&templateId="+l.id+"&access_token="+m.read("access_token")+"&format=pdf&locale="+e.locale+"&timezoneOffset="+(new Date).getTimezoneOffset()+"&save="+!0;p.get(r).then(function(e){l.link=e.data,"link"===a?tinymce.activeEditor.execCommand("mceInsertContent",!1,'<a href="'+e.data.fileurl+'">'+o+"</a>"):(t.attachmentLink=e.data.fileurl,t.attachmentName=o.substring(0,50),t.quoteTemplateName=" ( "+l.name+" ) "),t.templateAdding[a]=!1,i.setProgressState(!1)})}}},t.submitEMail=function(){if(t.emailModalForm.$valid){t.selectedIds=[],t.selectedIds.push(t.recordId),t.queryRequest={},t.queryRequest.query="*:*";var e="System"==t.senderAlias.type?1:3;t.submittingModal=!0;var l=function(){f.sendEMail(t.$parent.$parent.module.id,t.selectedIds,t.queryRequest.query,t.$parent.$parent.isAllSelected,t.tinymceModel,t.emailField.name,t.Cc,t.Bcc,t.senderAlias.alias,t.senderAlias.email,e,$,t.subject,t.attachmentLink,t.attachmentName).then(function(){t.submittingModal=!1,t.mailModal.hide(),t.$parent.$parent.isAllSelected=!1,t.$parent.$parent.selectedRows=[],t.$parent.$parent.emailSent&&t.$parent.$parent.emailSent(),a.create({content:n("translate")("EMail.MessageQueued"),className:"success"})})["catch"](function(){t.submittingModal=!1,t.mailModal.hide(),t.$parent.$parent.isAllSelected=!1,t.$parent.$parent.selectedRows=[],a.create({content:n("translate")("Common.Error"),className:"danger"})})};if("quotes"===t.$parent.$parent.module.name){var i={};i.id=t.$parent.$parent.record.id,"contacts"===t.emailField.moduleName?i.email=t.$parent.$parent.record.contact[t.emailField.name]:"accounts"===t.emailField.moduleName&&(i.email=t.$parent.$parent.record.account[t.emailField.name]),f.updateRecord("quotes",i).then(function(){l()})}else l()}}}]);