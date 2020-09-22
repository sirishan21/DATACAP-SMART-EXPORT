define([
		"dojo/_base/declare",
		"dojo/_base/json",
		"dijit/_TemplatedMixin",
		"dijit/_WidgetsInTemplateMixin",
		"ecm/widget/admin/PluginConfigurationPane",
		"dojo/i18n!./nls/messages",
		"ecm/widget/HoverHelp",
		"ecm/widget/ValidationTextBox",
		"dojo/text!./templates/ConfigurationPane.html"
	],
	function(declare, 
dojojson,
_TemplatedMixin, 
_WidgetsInTemplateMixin, 
PluginConfigurationPane,
messages, 
HoverHelp, 
ValidationTextBox,
template) {

		return declare("sETemplateBuilderPlugin.ConfigurationPane", [ PluginConfigurationPane, _TemplatedMixin, _WidgetsInTemplateMixin], {
		
		templateString: template,
		widgetsInTemplate: true,
		messages: messages,
	
		/**
		 * Initially load all the values from the configurationString onto the various fields.
		 */
				load : function(callback) {
					if (this.configurationString) {
						var jsonConfig = dojojson
								.fromJson(this.configurationString);
						this.dcServerURI.set('value', jsonConfig.server);
						this.dcPort.set('value', jsonConfig.port);
						this.dcUserName.set('value', jsonConfig.user);
						this.dcPassword.set('value', jsonConfig.password);
					}

				},

		/**
		 * Saves all the values from fields onto the configuration string which will be stored into the admin's configuration.
		 */
		save : function() {

			var configJson = {
					server : this.dcServerURI.get('value'),
					port : this.dcPort.get('value'),
					user : this.dcUserName.get('value'),
					password : this.dcPassword.get('value')
				}
			this.configurationString = dojojson.toJson(configJson);
			console.log(this.configurationString);
			this.onSaveNeeded(true);
		
		},

		/**
		 * Validates configuration data on configuration pane.
		 */
				validate : function() {
					if (!this.dcServerURI.isValid()) {
						return false;
					}
					if (!this.dcPort.isValid()) {
						return false;
					}
					if (!this.dcUserName.isValid()) {
						return false;
					}
					if (!this.dcPassword.isValid()) {
						return false;
					}
					return true;
				},

				_onFieldChange : function() {
					var configJson = {
						server : this.dcServerURI.get('value'),
						port : this.dcPort.get('value'),
						user : this.dcUserName.get('value'),
						password : this.dcPassword.get('value')
					};
					this.configurationString = dojojson.toJson(configJson);
					this.onSaveNeeded(true);
					console.log(this.configurationString);

				}

	});
});
