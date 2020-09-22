define([ "dojo/_base/declare",
		"dijit/_TemplatedMixin",
		"dijit/_WidgetsInTemplateMixin",
		"ecm/widget/admin/PluginConfigurationPane",
		"dojo/i18n!./nls/messages",
		"dojo/text!./templates/ConfigurationPane.html" ], 
		function(declare, _TemplatedMixin, _WidgetsInTemplateMixin, PluginConfigurationPane,messages,template) {

	return declare("sETemplateBuilderPlugin.FeatureConfigurationPane", [ PluginConfigurationPane, _TemplatedMixin,_WidgetsInTemplateMixin ], {

		templateString : template,
		widgetsInTemplate : true,
		messages: messages,
		/**
		 * Initially load all the values from the configurationString onto the various fields.
		 */
		load : function(callback) {

		},

		/**
		 * Saves all the values from fields onto the configuration string which will be stored into the admin's configuration.
		 */
		save : function() {

		},

		/**
		 * Validates configuration data on configuration pane.
		 */
		validate : function() {
			return true;
		}

	});
});
