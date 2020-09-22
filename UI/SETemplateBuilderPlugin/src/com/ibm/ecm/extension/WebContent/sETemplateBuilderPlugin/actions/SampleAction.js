define(["dojo/_base/declare", "ecm/model/Action"],
	function(declare, Action) {

	return declare("sETemplateBuilderPlugin.SampleAction", [ Action ], {
		/**
		 * Returns true if this action should be visible for the given repository, list type, and items.
		 */
		isVisible: function(repository, items, repositoryTypes, teamspace) {
			return true;
		},
		
		/**
		 * Returns true if this action should be enabled for the given repository, list type, and items.
		 */
		isEnabled: function(repository, listType, items, teamspace, resultSet) {
			return true;
		}
	});
});
