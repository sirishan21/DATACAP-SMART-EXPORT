package com.ibm.ecm.extension.services;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.Locale;
import javax.security.auth.Subject;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import com.filenet.api.core.ObjectStore;
import com.filenet.api.util.UserContext;
import com.ibm.ecm.extension.PluginLogger;
import com.ibm.ecm.extension.PluginResponseUtil;
import com.ibm.ecm.extension.PluginService;
import com.ibm.ecm.extension.PluginServiceCallbacks;
import com.ibm.ecm.json.JSONMessage;
import com.ibm.ecm.json.JSONResponse;
import com.ibm.ecm.serviceability.Logger;
import com.ibm.json.java.JSONArray;
import com.ibm.json.java.JSONObject;
import com.ibm.ecm.extension.utils.MessageResources;
import com.ibm.ecm.extension.services.Constants;

/**
 * Provides an abstract class that is extended to create a class implementing
 * each service provided by the plug-in. Services are actions, similar to
 * servlets or Struts actions, that perform operations on the IBM Content
 * Navigator server. A service can access content server application programming
 * interfaces (APIs) and Java EE APIs.
 * <p>
 * Services are invoked from the JavaScript functions that are defined for the
 * plug-in by using the <code>ecm.model.Request.invokePluginService</code>
 * function.
 * </p>
 * Follow best practices for servlets when implementing an IBM Content Navigator
 * plug-in service. In particular, always assume multi-threaded use and do not
 * keep unshared information in instance variables.
 */
public class P8SampleService extends PluginService {

	/**
	 * Returns the unique identifier for this service.
	 * <p>
	 * <strong>Important:</strong> This identifier is used in URLs so it must
	 * contain only alphanumeric characters.
	 * </p>
	 * 
	 * @return A <code>String</code> that is used to identify the service.
	 */
	public String getId() {
		return "P8SampleService";
	}

	/**
	 * Performs the action of this service.
	 * 
	 * @param callbacks
	 *            An instance of the <code>PluginServiceCallbacks</code> class
	 *            that contains several functions that can be used by the
	 *            service. These functions provide access to the plug-in
	 *            configuration and content server APIs.
	 * @param request
	 *            The <code>HttpServletRequest</code> object that provides the
	 *            request. The service can access the invocation parameters from
	 *            the request.
	 * @param response
	 *            The <code>HttpServletResponse</code> object that is generated
	 *            by the service. The service can get the output stream and
	 *            write the response. The response must be in JSON format.
	 * @throws Exception
	 *             For exceptions that occur when the service is running. If the
	 *             logging level is high enough to log errors, information about
	 *             the exception is logged by IBM Content Navigator.
	 */
	@Override
	public void execute(PluginServiceCallbacks callbacks,HttpServletRequest request, HttpServletResponse response) throws Exception {
		// Log execution
		PluginLogger logger = callbacks.getLogger();
		String methodName = "execute";

		logger.logEntry(this, methodName, request);
		String repositoryId = request.getParameter(Constants.PARM_REPOSITORY_ID);
		logger.logDebug(this, methodName, request, "repositoryId = " + repositoryId);
		
		String postContent = getRequestBody(request.getInputStream());
		JSONObject postJson = com.ibm.json.java.JSONObject.parse(postContent);
		//Get parameters from post content
		JSONResponse jsonResponse = new JSONResponse();
		JSONArray responseArray = new JSONArray();
		Locale locale = callbacks.getLocale();
		
		try {
				Subject subject = callbacks.getP8Subject(repositoryId);
				UserContext uc = UserContext.get();
				uc.pushSubject(subject);
				ObjectStore os = callbacks.getP8ObjectStore(repositoryId);

				Logger.logDebug(this, methodName, request, "Object store was retrieved");

					JSONObject osObject = new JSONObject();
					osObject.put("id",os.get_Id().toString());
					osObject.put("repositoryId",os.get_Name());
					responseArray.add(osObject);
					jsonResponse.put("objectStores",responseArray);
					JSONMessage successMsg = new JSONMessage(0,MessageResources.getMessage(locale, "service.retrieve.successful"), null,null,null,null);
					jsonResponse.addInfoMessage(successMsg);

		}
		catch (Exception e) {
			logger.logError(this, methodName, request, e);
			JSONMessage generalErrMsg = new JSONMessage(Integer.parseInt(MessageResources.getMessage(locale, "error.exception.general.id")),
					MessageResources.getMessage(locale, "error.exception.general"), 
					MessageResources.getMessage(locale, "error.exception.general.explanation"), 
					MessageResources.getMessage(locale, "error.exception.general.userResponse",new Object[] { e.getMessage() }), null, null);
			jsonResponse.addErrorMessage(generalErrMsg);
		}

		PluginResponseUtil.writeJSONResponse(request, response, jsonResponse, callbacks, "P8SampleService");
		logger.logExit(this, methodName, request);
	}
	

	private JSONMessage getMessage(Locale locale, String key){
		return new JSONMessage(Integer.parseInt(MessageResources.getMessage(locale, key+".id")),
				MessageResources.getMessage(locale, key), 
				MessageResources.getMessage(locale, key+".explanation"), 
				MessageResources.getMessage(locale, key+".userResponse"), null, null);
	}
	

	private String getRequestBody(InputStream inStream) throws IOException {
		BufferedReader reader = new BufferedReader(new InputStreamReader(
				inStream, "UTF-8"));

		StringBuffer buf = new StringBuffer();
		char data[] = new char[8196];
		int amtRead = 0;
		amtRead = reader.read(data, 0, 8196);
		while (amtRead != -1) {
			buf.append(data, 0, amtRead);
			amtRead = reader.read(data, 0, 8196);
		}
		String s = buf.toString().trim();
		return s;
	}
	/**
	 * Validate that the request coming in from the client includes all parameters and can be handled by this service.
	 * @param callbacks
	 * @param request
	 * @param jsonResponse
	 * @return true if the request has all required parameters, false otherwise.
	 */
	private boolean validateServiceRequest(PluginServiceCallbacks callbacks, HttpServletRequest request, JSONResponse jsonResponse) {
		PluginLogger logger = callbacks.getLogger();
		logger.logEntry(this, "validateServiceRequest");
		String methodName = "validateRequest";
		logger.logEntry(this, methodName, request);

		// Get all the required parameters for validation
		String repositoryId = request.getParameter(Constants.PARM_REPOSITORY_ID);
		logger.logDebug(this, methodName, request, "Request Parameter: repositoryId = " + repositoryId);

		// Validate repository id
		if (repositoryId == null || repositoryId.trim().length() < 1) {
			return false;
		}

		logger.logExit(this, methodName, request);

		return true;
	}


}
