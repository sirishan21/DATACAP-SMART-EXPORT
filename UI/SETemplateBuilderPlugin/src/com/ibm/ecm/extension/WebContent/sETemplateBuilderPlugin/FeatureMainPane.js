define([
	"dojo/_base/declare",
	"dojo/query",
	"ecm/widget/layout/_LaunchBarPane",
	"dojo/text!./templates/FeatureMainPane.html"],
function(declare, query, _LaunchBarPane, template) {
	return declare("sETemplateBuilderPlugin.FeatureMainPane", [ _LaunchBarPane], {
		templateString: template,
		widgetsInTemplate: true,
		
		loadContent: function() {
			this.logEntry("loadContent");

			var container = query(".sETemplateBuilderFeature")[0];

			if (window.icnReactService) {
                window.icnReactService.sendMessage({
					message: 'remove_sETemplateBuilderFeature',
					containerId: container
                });
                
                window.icnReactService.sendMessage({
					message: 'render_sETemplateBuilderFeature',
					containerId: container
                });
			}
			this.logExit("loadContent");
		}

	});
});
