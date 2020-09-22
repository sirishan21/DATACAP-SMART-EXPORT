require(["dojo/_base/declare",
		"dojo/_base/lang",
         "dojo/i18n!sETemplateBuilderPlugin/nls/messages"], function(declare, lang, messages) {
	
	console.log("Loading SETemplateBuilderPlugin...");

	var loadCSS = function(cssFileUrl){
		if (dojo.isIE) {
			document.createStyleSheet(cssFileUrl);
		} else {
			var head = document.getElementsByTagName("head")[0];
			var link = document.createElement("link");
			link.rel = "stylesheet";
			link.type = "text/css";
			link.href = cssFileUrl;
			head.appendChild(link);
		}
	};

	var loadJS = function(src, callback){
        const script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = src;
        script.onload = callback;
        document.body.appendChild(script);
	};
	sETemplateBuilderPlugin ={};
	sETemplateBuilderPlugin.messages = messages;

	//Load css files from React components
	loadCSS("./plugin/SETemplateBuilderPlugin/getResource/sETemplateBuilderPlugin/build/icn-react.min.css");

	//Load js files for React code
	loadJS("plugin/SETemplateBuilderPlugin/getResource/sETemplateBuilderPlugin/build/icn-react.min.js", function(){

		//Define sample action
		lang.setObject("icnSampleAction", function(repository, items, callback, teamspace, resultSet, parameterMap) {
			console.log(sETemplateBuilderPlugin.messages.sample_action_message);
		});


	});

});
