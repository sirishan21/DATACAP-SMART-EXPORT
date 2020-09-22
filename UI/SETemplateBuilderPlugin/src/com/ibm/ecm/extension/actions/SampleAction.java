package com.ibm.ecm.extension.actions;

import java.util.Locale;
import com.ibm.ecm.extension.PluginAction;
import com.ibm.json.java.JSONObject;
import com.ibm.ecm.extension.utils.MessageResources;

public class SampleAction extends PluginAction {

	@Override
	public String getActionFunction() {
		return "icnSampleAction";
	}

	@Override
	public String getIcon() {
		return "";
	}
	public String getIconClass() {
		return "";
	}
	@Override
	public String getId() {
		return "icnSampleAction";
	}

	@Override
	public String getName(Locale locale) {
		return MessageResources.getMessage(locale, "icnSampleAction.name");
	}

	@Override
	public String getPrivilege() {
		return "";
	}

	@Override
	public String getServerTypes() {
		return "";
	}

	@Override
	public boolean isMultiDoc() {
		return true;
	}
	
	public boolean isGlobal() {
		return true;
	}
	
//	public String[] getMenuTypes() {
//		return new String[]{"ItemContextMenu","TeamspaceItemContextMenu"};
//	}
	
	public JSONObject getAdditionalConfiguration(Locale locale) {
		return new JSONObject();
	}
	
	public String getActionModelClass() {
		return "sETemplateBuilderPlugin/actions/SampleAction";
	}
}
