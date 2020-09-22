package com.ibm.ecm.extension.utils;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.ProtocolException;
import java.net.URL;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import org.w3c.dom.Document;
import org.w3c.dom.NodeList;
import org.w3c.dom.Node;
import org.xml.sax.SAXException;

import com.ibm.ecm.extension.PluginLogger;
import com.ibm.json.java.JSONArray;
import com.ibm.json.java.JSONObject;
 

/**
 * This is a utility class that enables one to get datacap application and DCO related 
 * information using the datacap rest services.
 *
 */
public class DatacapService {

	private String datacapServiceURL;
	
	private Document dcoDocument;

	private PluginLogger logger;

	public DatacapService(PluginLogger logger) {
		this.logger = logger;
	}
	
	/**
	 * Returns the DCO structural information in the form of a JSON for the
	 * specified application.
	 * 
	 * @param application         datacap application name.
	 * @param configurationString contains the datcap service login information
	 * @return the DCO structural information in the form of a JSON for the
	 *         specified application.
	 * @throws DatacapServiceException
	 */
	public JSONObject getDCOasJSONForApplication(String application, String configurationString)
			throws SETemplateBuilderException {
		String methodName = "DatacapService.getDCOasJSONForApplication";
		logger.logEntry(this, methodName);
		JSONObject dcoAsJSON = new JSONObject();
		if (application == null || application.isEmpty()) {
			logger.logError(this, methodName, 
					"SETemplateBuilder : It's mandatory to provide the application name.");
			throw new SETemplateBuilderException("It's mandatory to provide the application name.");

		} else if (configurationString == null || configurationString.isEmpty()) {
			logger.logError(this, methodName, 
					"SETemplateBuilder :It's mandatory to provide the datacap server and login details.");
			throw new SETemplateBuilderException("It's mandatory to provide the datacap server and login details.");

		} else {
 			logger.logEntry(this, methodName);
			loadDCOFile(application, configurationString);
			JSONArray documents = getDocuments();
			dcoAsJSON.put(Constants.DOCUMENTS, documents);
			JSONObject documentPageMap = new JSONObject();
			JSONObject pageFieldMap = new JSONObject();
			for (int i = 0; i < documents.size(); i++) {
				String document = (String) ((JSONObject) (documents.get(i))).get(Constants.ID);
				JSONArray pagesOfDocument = getPagesOfDocument(document);
				documentPageMap.put(document, pagesOfDocument);
				for (int j = 0; j < pagesOfDocument.size(); j++) {
					String page = (String) ((JSONObject) (pagesOfDocument.get(j))).get(Constants.ID);
					JSONArray fieldsOfPage = getFieldsOfPage(page, document);
					pageFieldMap.put(page, fieldsOfPage);
				}
			}
			dcoAsJSON.put(Constants.DOCUMENT_PAGE_MAP, documentPageMap);
			dcoAsJSON.put(Constants.PAGE_FIELD_MAP, pageFieldMap);
			logger.logInfo(this, methodName, "SETemplateBuilder : DCO as JSON : \n" + dcoAsJSON);
			logger.logExit(this, methodName);
		}
		return dcoAsJSON;
	}

	/**
	 * Returns a list of document types present in the application.
	 * @return list of document types present in the application.
	 */
	private JSONArray getDocuments() {
		JSONArray documents = new JSONArray();
		Node batchNode = dcoDocument.getElementsByTagName(Constants.BATCH_TAG).item(0);
		NodeList nodes = batchNode.getChildNodes();
		for (int i = 0; i < nodes.getLength(); i++) {
			Node childNode = nodes.item(i);
			if (childNode.getNodeName() == Constants.DOCUMENT_TAG) {
				JSONObject document = new JSONObject();
				document.put(Constants.ID,
						childNode.getAttributes().getNamedItem(Constants.TYPE_ATTRIBUTE).getNodeValue());
				document.put(Constants.NAME,
						childNode.getAttributes().getNamedItem(Constants.TYPE_ATTRIBUTE).getNodeValue());
				documents.add(document);
			}
		}
		return documents;
	}

	/**
	 * Returns all page types present in the specified document type.
	 * @param document document type
	 * @return list of  page types present in the specified document type.
	 */
	private JSONArray getPagesOfDocument(String document) {
		JSONArray pages = new JSONArray();
		Node documentNode = null;

		Node batchNode = dcoDocument.getElementsByTagName(Constants.BATCH_TAG).item(0).getParentNode();
		NodeList nodes = batchNode.getChildNodes();
		for (int i = 0; i < nodes.getLength(); i++) {
			Node childNode = nodes.item(i);
			if (childNode.getNodeName().equals(Constants.DOCUMENT_TAG)) {
				if (childNode.getAttributes().getNamedItem(Constants.TYPE_ATTRIBUTE).getNodeValue().equals(document)) {
					documentNode = childNode;
					break;
				}
			}

		}
		if (null != documentNode) {
			NodeList pageNodes = documentNode.getChildNodes();
			for (int i = 0; i < pageNodes.getLength(); i++) {
				Node childNode = pageNodes.item(i);
				if (childNode.getNodeName().equals(Constants.PAGE_TAG)) {
					JSONObject page = new JSONObject();
					page.put(Constants.ID,
							childNode.getAttributes().getNamedItem(Constants.TYPE_ATTRIBUTE).getNodeValue());
					page.put(Constants.NAME,
							childNode.getAttributes().getNamedItem(Constants.TYPE_ATTRIBUTE).getNodeValue());
					pages.add(page);

				}

			}
		}
		return pages;
	}

	/**
	 * Returns a map of DCO expression and field name for all of the fields present in the page.
	 * @param page page type
	 * @param document document type
	 * @return map of DCO expression and field name for all of the fields present in the page
	 */
	private JSONArray getFieldsOfPage(String page, String document) {
		JSONArray fields = new JSONArray();

		Node pageNode = null;
		String pageName = "";
		Node batchNode = dcoDocument.getElementsByTagName(Constants.BATCH_TAG).item(0).getParentNode();
		NodeList nodes = batchNode.getChildNodes();
		for (int i = 0; i < nodes.getLength(); i++) {
			Node childNode = nodes.item(i);
			if (childNode.getNodeName().equals(Constants.PAGE_TAG)) {
				pageName = childNode.getAttributes().getNamedItem(Constants.TYPE_ATTRIBUTE).getNodeValue();
				if (pageName.equals(page)) {
					pageNode = childNode;
					break;
				}
			}

		}
		if (null != pageNode) {
			NodeList pageNodes = pageNode.getChildNodes();
			for (int i = 0; i < pageNodes.getLength(); i++) {
				Node childNode = pageNodes.item(i);
				if (childNode.getNodeName().equals(Constants.FIELD_TAG)) {
					String fieldName = childNode.getAttributes().getNamedItem(Constants.TYPE_ATTRIBUTE).getNodeValue();
					JSONObject field = new JSONObject();
					field.put(Constants.ID, "[DCO].[" + document + "].[" + pageName + "].[" + fieldName + "]");
					field.put(Constants.NAME, fieldName);
					fields.add(field);
				}
			}
		}
		return fields;

	}

	/**
	 * Returns names of all the application on the datacap server.
	 * @param configurationString contains the datcap service login information
	 * @return names of all the application on the datacap server.
	 * @throws SETemplateBuilderException 
	 */
	public JSONObject getApplications(String configurationString) throws SETemplateBuilderException {
		String methodName = "DatacapService.getApplications";
		logger.logEntry(this, methodName);
		
		if (configurationString == null || configurationString.isEmpty()) {
			logger.logError(this, methodName,
					"SETemplateBuilder :It's mandatory to provide the datacap server and login details.");
			throw new SETemplateBuilderException("It's mandatory to provide the datacap server and login details.");

		}
		JSONObject jsonObject = null;
		try {
			JSONObject configJSON = JSONObject.parse(configurationString);

			setURL(configJSON);

			HttpURLConnection connection = getConnection(datacapServiceURL + "/Admin/GetApplicationList", null);

			jsonObject = JSONObject.parse(new InputStreamReader(connection.getInputStream(), "UTF-8"));
			logger.logInfo(this, methodName, "SETemplateBuilder : Applications : \n" + jsonObject.get("Applications"));
			connection.disconnect();
			logger.logExit(this, methodName);
		} catch (Exception exception) {
			exception.printStackTrace();
			logger.logError(this, methodName, "SETemplateBuilder : Unable to fetch Datacap application list.");
			throw new SETemplateBuilderException("Unable to fetch Datacap application list.");
		}
		return jsonObject;
	}

	/**
	 * Gets the HTTP URL connection object.
	 * @param path rest endpoint
	 * @param cookie session information
	 * @return   HTTP URL connection object.
	 * @throws MalformedURLException
	 * @throws IOException
	 * @throws ProtocolException
	 */
	private  HttpURLConnection getConnection(String path, String cookie)
			throws MalformedURLException, IOException, ProtocolException {
		URL url = new URL(path);
		HttpURLConnection connection = (HttpURLConnection) url.openConnection();

		connection.setRequestMethod("GET");
		connection.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");
		connection.setRequestProperty("Accept", "application/json");
		if (cookie != null) {
			connection.setRequestProperty("cookie", cookie);
		}
		connection.setUseCaches(false);
		if (connection.getResponseCode() != 200) {
			throw new RuntimeException("Failed : HTTP error code : " + connection.getResponseCode());
		}
		return connection;
	}

	/**
	 * Loads the specified datacap application's DCO file.
	 * @param applicationName   datacap application name
	 * @param configurationString contains the login details
	 * @throws DatacapServiceException 
	 */
	private void loadDCOFile(String applicationName, String configurationString) throws SETemplateBuilderException {
		
		String methodName = "DatacapService.loadDCOFile";

		try {
			
			logger.logEntry(this, methodName);
			
			JSONObject configJSON =   JSONObject.parse(configurationString);
			
			setURL(configJSON);
			
			String cookie = logon(applicationName,(String) configJSON.get("user"),(String) configJSON.get("password"));
			
			HttpURLConnection conn = getConnection(
					datacapServiceURL + "/Admin/GetSetupDCOFile/" 
			+ applicationName + "/" + applicationName, cookie);

			InputStream input = conn.getInputStream();

			DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
			DocumentBuilder parser = factory.newDocumentBuilder();
			dcoDocument = parser.parse(input);

			conn.disconnect();			
			
			logger.logExit(this, methodName);

		} catch (IOException | ParserConfigurationException | SAXException exception) {
			exception.printStackTrace();
			logger.logError(this, methodName, 
					"SETemplateBuilder : Unable to fetch Datacap DCO file.");
			throw new SETemplateBuilderException("Unable to fetch Datacap DCO file.");
		}

	}

	/**
	 * Sets the URL for datacap rest service.
	 * 
	 * @param configJSON contains the server details of the datacap rest service.
	 */
	private void setURL(JSONObject configJSON) {
		String methodName = "DatacapService.setURL";
		logger.logEntry(this, methodName);

		datacapServiceURL = Constants.REST_API_HTTP_PREFIX +  (String) configJSON.get("server") 
				+ Constants.COLON_NO_SPACE +  (String) configJSON.get("port") + Constants.REST_API_SERVICE;
		logger.logInfo(this, methodName, "SETemplateBuilder : Datacap service URL is : \n" + datacapServiceURL);
		logger.logExit(this, methodName);

	}

	/**
	 * Logs onto the datacap application server.
	 * @param applicationName name of the datacap application.
	 * @param userName user name
	 * @param password password
	 * @return session information
	 * @throws SETemplateBuilderException 
	 */
	private String logon(String applicationName, String userName, String password) throws SETemplateBuilderException {

		String methodName = "DatacapService.logon";
		logger.logEntry(this, methodName);

		String cookie = null;
		try {

			URL url = new URL(datacapServiceURL + "/Session/Logon");
			HttpURLConnection conn = (HttpURLConnection) url.openConnection();
			conn.setDoOutput(true);
			conn.setRequestMethod("POST");
			conn.setRequestProperty("Content-Type", "application/json");

			String input = "{\n" + "	\"application\":\"" + applicationName 
					+ "\",\n" + "	\"password\":\"" + password
					+ "\",\n" + "	\"station\":\"" + "1" 
					+ "\",\n" + "	\"user\":\"" + userName + "\"\n" + "}";

			OutputStream os = conn.getOutputStream();
			os.write(input.getBytes());
			os.flush();
			
			logger.logInfo(this, methodName, "SETemplateBuilder : Response for datacap service logon request : " + conn.getResponseCode());
			
			if (conn.getResponseCode() != HttpURLConnection.HTTP_OK) {
				BufferedReader br = new BufferedReader(new InputStreamReader((conn.getErrorStream())));

				String output;
				logger.logError(this, methodName, "Error from Server .... \n");
				while ((output = br.readLine()) != null) {
					logger.logError(this, methodName, output);
				}
				logger.logError(this, methodName, "SETemplateBuilder : Failed : HTTP error code : " + conn.getResponseCode());
				throw new RuntimeException("Failed : HTTP error code : " + conn.getResponseCode());

			}

			BufferedReader br = new BufferedReader(new InputStreamReader((conn.getInputStream())));

			String output;

			while ((output = br.readLine()) != null) {
				logger.logInfo(this, methodName, output);
			}
			cookie = conn.getHeaderField("Set-Cookie");

			logger.logInfo(this, methodName, "Output from Server .... \\n" + cookie);

			conn.disconnect();

			logger.logExit(this, methodName);

		} catch (IOException e) {
			e.printStackTrace();
			logger.logError(this, methodName, "SETemplateBuilder : Unable to logon to the datacap service.");
			throw new SETemplateBuilderException("Unable to logon to the datacap service.");

		}
		return cookie;

	}

}
