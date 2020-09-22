package com.ibm.ecm.extension.utils;

import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.TransformerFactoryConfigurationError;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import org.apache.commons.lang.StringUtils;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Text;

import com.ibm.ecm.extension.PluginLogger;
import com.ibm.json.java.JSONArray;
import com.ibm.json.java.JSONObject;

/**
 * This is a utility class that enables one build a Smart Export XML template
 * that can be used in the Datacap Smart Export custom action.
 *
 */
public class TemplateBuilder {

	private String delimeter = Constants.COMMA;
	private String level;
	private String format;
	private PluginLogger logger;
	private List<String> headers = new ArrayList<String>();
	
	public TemplateBuilder(PluginLogger logger) {
		this.logger = logger;
	}	

	/**
	 * Builds the smart export template based on the template details that was
	 * received.
	 * 
	 * @param templateDetails the details that will be used to build the smart
	 *                        export template.
	 * @return the smart export template as String.
	 * @throws TemplateBuilderException
	 */
	public String buildTemplate(JSONObject templateDetails) throws SETemplateBuilderException {
	   
		String methodName = "TemplateBuilder.buildTemplate";
		logger.logEntry(this, methodName);
		logger.logInfo(this, methodName, "SETemplateBuilder : The input received is : \n"+templateDetails);

		String template = Constants.EMPTY_STRING;
		DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		DocumentBuilder builder;
		try {
			builder = factory.newDocumentBuilder();
			Document doc = builder.newDocument();
			Element mainRootElement = doc.createElementNS(Constants.XML_SCHEMA_NAMESPACE, Constants.SE_SMARTEXPORT);
			doc.appendChild(mainRootElement);

			// append child elements to root element
			addConfigElements(doc, mainRootElement, templateDetails);
		    delimeter = (String) templateDetails.get(Constants.DELIMETER);
			level = (String) templateDetails.get(Constants.LEVEL);
			if (format.equalsIgnoreCase(Constants.CSV)|| level.equalsIgnoreCase(Constants.FIELD_LEVEL)) {
				addHeaderTag(doc, mainRootElement, templateDetails);
			}
			addElementsBasedOnLevel(level, doc, mainRootElement,
					templateDetails);
			// output DOM XML to string
			Transformer transformer = TransformerFactory.newInstance().newTransformer();
			transformer.setOutputProperty(OutputKeys.INDENT, Constants.YES);
			transformer.setOutputProperty(Constants.XSLT_INDENT_AMOUNT, Constants.FOUR);
			transformer.setOutputProperty(OutputKeys.OMIT_XML_DECLARATION, Constants.YES);
			DOMSource source = new DOMSource(doc);
			StringWriter writer = new StringWriter();
			StreamResult result = new StreamResult(writer);
			transformer.transform(source, result);
			template = writer.toString();
			logger.logInfo(this, methodName, "SETemplateBuilder : The generated SETemplate is : \n"+template);
			logger.logExit(this, methodName);

			
		} catch (TransformerFactoryConfigurationError | Exception e) {
			e.printStackTrace();
			logger.logError(this, methodName, 
					"SETemplateBuilder : Unable to generate the Template XML. " + "Please recheck the input values.");
			throw new SETemplateBuilderException(
					"Unable to generate the Template XML. " + "Please recheck the input values.");
		}
		return template;

	}


	/**
	 * Adds elements based on the DCO level at which the template is to be
	 * associated.
	 * 
	 * @param level           the DCO level at which the template is to be
	 *                        associated.
	 * @param doc             document object of the XML template.
	 * @param mainRootElement root element of the XML template.
	 * @param templateDetails the details that will be used to build the page level
	 *                        XML elements.
	 */
	private void addElementsBasedOnLevel(String level, Document doc, Element mainRootElement,
			JSONObject templateDetails) {
		switch (level) {
		case Constants.PAGE_LEVEL:
			addPageLevelElements(doc, mainRootElement, templateDetails);
			break;
		case Constants.FIELD_LEVEL:
			addFieldLevelElements(doc, mainRootElement, templateDetails);
			break;
		case Constants.DOCUMENT_LEVEL:
			addDocumentLevelElements(doc, mainRootElement, templateDetails);
			break;
		case Constants.BATCH_LEVEL:
			addBatchLevelElements(doc, mainRootElement, templateDetails);
			break;
		default:
			break;
		}
	}

	/**
	 * Add elements that are pertaining to a batch level template
	 * 
	 * @param doc             document object of the XML template.
	 * @param mainRootElement root element of the XML template.
	 * @param templateDetails the details that will be used to build the batch level
	 *                        XML elements.
	 */
	private void addBatchLevelElements(Document doc, Element mainRootElement, JSONObject templateDetails) {
		Element forTag = doc.createElement(Constants.SE_FOR);
		forTag.setAttribute(Constants.SELECT, Constants.DOCUMENT_LEVEL);
		mainRootElement.appendChild(forTag);
		addNewLineTag(doc, forTag);
		addPageLevelElements(doc, forTag, templateDetails);
	}

	/**
	 * Add elements that are pertaining to a document level template
	 * 
	 * @param doc             document object of the XML template.
	 * @param mainRootElement root element of the XML template.
	 * @param templateDetails the details that will be used to build the page level
	 *                        XML elements.
	 */
	private void addDocumentLevelElements(Document doc, Element mainRootElement, JSONObject templateDetails) {
		// build the page details of the document
		Element forTag = doc.createElement(Constants.SE_FOR);
		forTag.setAttribute(Constants.SELECT, Constants.PAGE);
		mainRootElement.appendChild(forTag);
		if (level.equals(Constants.DOCUMENT_LEVEL)) {
			addNewLineTag(doc, forTag);
		}
		addPageLevelElements(doc, forTag, templateDetails);
	}

	/**
	 * Add elements that are pertaining to a field level template
	 * 
	 * @param doc             document object of the XML template.
	 * @param mainRootElement root element of the XML template.
	 * @param templateDetails the details that will be used to build the page level
	 *                        XML elements.
	 */
	private void addFieldLevelElements(Document doc, Element mainRootElement, JSONObject templateDetails) {
		Element ifConditionsTag = null;
		JSONArray conditonGroup = (JSONArray) templateDetails.get(Constants.CONDITION_GROUPS);
		if (null != conditonGroup && conditonGroup.size() > 0) {
			
			if (null != ((JSONObject) conditonGroup.get(0)).get(Constants.CONDITIONS)) {
				JSONArray conditions = (JSONArray) ((JSONObject) conditonGroup.get(0)).get(Constants.CONDITIONS);
				ifConditionsTag = addConditionTag(doc, conditions);
			}
			Element dataParentTag = null;
			if (ifConditionsTag != null) {
				mainRootElement.appendChild(ifConditionsTag);
				dataParentTag = ifConditionsTag;
			} else {
				dataParentTag = mainRootElement;
			}
			if ( null != ((JSONObject) conditonGroup.get(0)).get(Constants.FIELDS)) {
				JSONArray fields = (JSONArray) ((JSONObject) conditonGroup.get(0)).get(Constants.FIELDS);
				addDataTags(doc, dataParentTag, fields);
			} 
		}
	}

	/**
	 * Add elements that are pertaining to a page level template
	 * 
	 * @param doc             document object of the XML template.
	 * @param mainRootElement root element of the XML template.
	 * @param templateDetails the details that will be used to build the page level
	 *                        XML elements.
 	 */
	private void addPageLevelElements(Document doc, Element mainRootElement, JSONObject templateDetails) {
		if (level.equals(Constants.PAGE_LEVEL)) {
			addNewLineTag(doc, mainRootElement);
		}
		// generate the condition and its corresponding data tags
		if (null != templateDetails.get(Constants.CONDITION_GROUPS)) {
			JSONArray conditionGroups = (JSONArray) templateDetails.get(Constants.CONDITION_GROUPS);
			JSONArray fields = null;
			for (int i = 0; i < conditionGroups.size(); i++) {
				JSONObject conditionGroup = (JSONObject) conditionGroups.get(i);
				Element ifConditionsTag = null;
				Element elseTag = null;
				if (null != conditionGroup.get(Constants.CONDITIONS)) {
					JSONArray conditions = (JSONArray) conditionGroup.get(Constants.CONDITIONS);
					ifConditionsTag = addConditionTag(doc, conditions);
				}

				Element dataParentTag = null;
				if (ifConditionsTag != null) {
					mainRootElement.appendChild(ifConditionsTag);
					dataParentTag = ifConditionsTag;
				} else {
					dataParentTag = mainRootElement;
				}

				// add the data elements
				if (null != conditionGroup.get(Constants.FIELDS)) {
					fields = (JSONArray) conditionGroup.get(Constants.FIELDS);
					addDataTags(doc, dataParentTag, fields);
				}
				if (ifConditionsTag != null && format.equalsIgnoreCase(Constants.CSV)) {
					elseTag = doc.createElement(Constants.SE_ELSE);
					ifConditionsTag.appendChild(elseTag);
					addEmptyDataTags(doc, elseTag, fields.size());

				}
			}
		}
	}

	/**
	 * Creates an if tag in the XML template.
	 * 
	 * @param doc        document object of the XML template.
	 * @param conditions contains the condition details.
	 * @param tagName    name of the condition tag
	 * @return the condition element.
	 */
	private Element addConditionTag(Document doc, JSONArray conditions, String tagName) {
		Element ifConditionsTag = null;
		if (conditions.size() > 0) {
			ifConditionsTag = doc.createElement(tagName);
			StringBuilder conditionSB = new StringBuilder();
			for (int i = 0; i < conditions.size(); i++) {
				JSONObject condition = (JSONObject) conditions.get(i);
				String joinOperator = ((String) condition.get(Constants.JOIN_OPERATOR)).toLowerCase();
				if(joinOperator.equals(Constants.BLANK))
					joinOperator=Constants.EMPTY_STRING;
				conditionSB
						.append((String) condition.get(Constants.FIELD)).append(Constants.SPACE)
						.append(((String) condition.get(Constants.OPERATOR)).toUpperCase().replace(Constants.SPACE,
								Constants.HYPHEN))
						.append(Constants.SPACE).append((String) condition.get(Constants.VALUE))
						.append(Constants.SPACE)
						.append(joinOperator).append(Constants.SPACE);
			}
			ifConditionsTag.setAttribute(Constants.TEST, conditionSB.toString().trim());
		}
		return ifConditionsTag;
	}

	/**
	 * Creates an if tag in the XML template.
	 * 
	 * @param doc        document object of the XML template.
	 * @param conditions contains the condition details.
	 * 
	 * @return the condition element.
	 */
	private Element addConditionTag(Document doc, JSONArray conditions) {
		return addConditionTag(doc, conditions, Constants.SE_IF);
	}

	/**
	 * Adds data tags in the XML template.
	 * 
	 * @param doc           document object of the XML template.
	 * @param dataParentTag the parent tag to which the newly created data elements
	 *                      will be children.
	 * @param fields        contains the details of the data fields to be included
	 *                      in the XML template.
	 */
	private void addDataTags(Document doc, Element dataParentTag, JSONArray fields) {

		if (null != fields && fields.size() > 0) {

			String delimeterName = Constants.SE_NEW_LINE;
			if (format.equalsIgnoreCase(Constants.CSV)) {
				delimeterName = (delimeter != null && delimeter.equalsIgnoreCase(Constants.COMMA))
						? (Constants.SE_COMMA)
						: (Constants.SE_TAB);

			}
			for (int j = 0; j < fields.size(); j++) {
				// data separator
				Element data = doc.createElement(Constants.SE_DATA);
				JSONObject fieldJSON = (JSONObject) fields.get(j);
				JSONObject field = (JSONObject) fieldJSON.get(Constants.FIELD);

				if (!format.equalsIgnoreCase(Constants.CSV) && !level.equalsIgnoreCase(Constants.FIELD_LEVEL)) {
					// label
					Text text = doc.createTextNode((String) field.get(Constants.KEY) + delimeter);
					data.appendChild(text);
				}
				Element value = doc.createElement(Constants.SE_VALUE);
				value.setAttribute(Constants.SELECT, (String) field.get(Constants.VALUE));
				data.appendChild(value);
				if (!level.equalsIgnoreCase(Constants.FIELD_LEVEL)) {
					Element delimeterTag = doc.createElement(delimeterName);
					data.appendChild(delimeterTag);
				}
				dataParentTag.appendChild(data);
			}
		}
	}

	/**
	 * Adds new line tag in the XML template.
	 * 
	 * @param doc           document object of the XML template.
	 * @param dataParentTag the parent tag to which the newly created data elements
	 *                      will be children.
	 */
	private void addNewLineTag(Document doc, Element dataParentTag) {
		if (!format.equalsIgnoreCase(Constants.CSV)|| level.equals(Constants.BATCH_LEVEL)) {
			Element newLine = doc.createElement(Constants.SE_NEW_LINE);
			Element newLineData = doc.createElement(Constants.SE_DATA);
			newLineData.appendChild(newLine);
			dataParentTag.appendChild(newLineData);
		}
	}
	
	
	/**
	 * Adds data tags in the XML template.
	 * 
	 * @param doc           document object of the XML template.
	 * @param insertBeforeTag the parent tag to which the newly created data elements
	 *                      will be children.
	 * 
	 */
	private void addHeaderTag(Document doc, Element mainRootElement, JSONObject templateDetails) {
		JSONArray fields = new JSONArray();

		if (null != templateDetails.get(Constants.CONDITION_GROUPS)) {
			JSONArray conditionGroups = (JSONArray) templateDetails.get(Constants.CONDITION_GROUPS);
			for (int i = 0; i < conditionGroups.size(); i++) {
				JSONObject conditionGroup = (JSONObject) conditionGroups.get(i);

				// add the label elements
				if (null != conditionGroup.get(Constants.FIELDS)) {
					JSONArray fieldsArray = ((JSONArray) conditionGroup.get(Constants.FIELDS));
					for (int j = 0; j < fieldsArray.size(); j++) {
						fields.add((JSONObject) fieldsArray.get(j));
					}

				}
			}
		}

		populateHeadersFromFields(fields);
		if (!headers.isEmpty()) {
			Element headerData = doc.createElement(Constants.SE_HEADER);
			for (String header : headers) {

				Text text = doc.createTextNode(header);
				headerData.appendChild(text);
				Element delimeterTag = (delimeter.equalsIgnoreCase(Constants.COMMA))
						? doc.createElement(Constants.SE_COMMA)
						: doc.createElement(Constants.SE_TAB);
				headerData.appendChild(delimeterTag);
				mainRootElement.appendChild(headerData);

			}
			mainRootElement.appendChild(headerData);
		}
	}
	
	/**
	 * Extract header names from fields json.
	 * @param fields        contains the details of the data fields to be included
	 *                      in the XML template.
	 */
	private void populateHeadersFromFields(JSONArray fields) {
		for(int i = 0; i < fields.size(); i++) {
			JSONObject fieldJSON = (JSONObject) fields.get(i);
			JSONObject	field=(JSONObject)	fieldJSON.get(Constants.FIELD);
			headers.add((String)field.get(Constants.KEY));
		}
	}

	/**
	 * Adds data tags in the XML template.
	 * 
	 * @param doc           document object of the XML template.
	 * @param dataParentTag the parent tag to which the newly created data elements
	 *                      will be children.
	 * @param fields        contains the details of the data fields to be included
	 *                      in the XML template.
	 */
	private void addEmptyDataTags(Document doc, Element dataParentTag, int fieldCount) {

		if (fieldCount > 0) {
			for (int j = 0; j < fieldCount; j++) {
				Element data = doc.createElement(Constants.SE_DATA);
				// label
				Text text = doc.createTextNode(Constants.SPACE);
				data.appendChild(text);
				Element delimeterTag = (delimeter.equalsIgnoreCase(Constants.COMMA))
						? doc.createElement(Constants.SE_COMMA)
						: doc.createElement(Constants.SE_TAB);
				data.appendChild(delimeterTag);
				dataParentTag.appendChild(data);
			}
		}
	}

	/**
	 * Adds configuration elements in the XML template.
	 * 
	 * @param doc             document object of the XML template.
	 * @param mainRootElement root element of the XML template.
	 * @param templateDetails the details that will be used to build the
	 *                        configuration XML element.
	 */
	private void addConfigElements(Document doc, Element mainRootElement, JSONObject templateDetails) {

		Element append = doc.createElement(Constants.SE_APPEND_TO_FILE);
		append.appendChild(doc.createTextNode((String) templateDetails.get(Constants.APPEND_TO_FILE)));
		mainRootElement.appendChild(append);

		Element localeTag = doc.createElement(Constants.SE_LOCALE);
		JSONObject localeObject = (JSONObject) templateDetails.get(Constants.LOCALE);
		addTagsForSmartParamElements(doc, mainRootElement, localeTag, localeObject);

		Element fileNameTag = doc.createElement(Constants.SE_FILENAME);
		JSONObject fileObject = (JSONObject) templateDetails.get(Constants.FILE_NAME);
		addTagsForSmartParamElements(doc, mainRootElement, fileNameTag, fileObject);

		Element exportFormat = doc.createElement(Constants.SE_FILEEXT);
		format=(String) templateDetails.get(Constants.EXPORT_FORMAT);
		format=format.equals(Constants.KeyValue)?Constants.TXT:format;
		exportFormat.appendChild(doc.createTextNode(format));
		mainRootElement.appendChild(exportFormat);

		Element outputFolderTag = doc.createElement(Constants.SE_OUTPUT_FOLDER);
		JSONObject folderObject = (JSONObject) templateDetails.get(Constants.OUTPUT_FOLDER);
		addTagsForSmartParamElements(doc, mainRootElement, outputFolderTag, folderObject);

	}

	/**
	 * Adds XML element that has a smart parameter associated to the main XML root
	 * element.
	 * 
	 * @param doc             Document object of the XML template.
	 * @param mainRootElement root element of the XML template.
	 * @param currentElement  current element XML template that has a smart
	 *                        parameter associated.
	 * @param currentDetails  the details that will be used to build the current XML
	 *                        element.
	 */
	private void addTagsForSmartParamElements(Document doc, Element mainRootElement, Element currentElement,
			JSONObject currentDetails) {
		if (null != currentDetails) {
			String value = (String) currentDetails.get(Constants.VALUE);
			String smartparam = (String) currentDetails.get(Constants.SMART_PARAM);
			if (value != null && StringUtils.isNotEmpty(value)) {
				currentElement.appendChild(doc.createTextNode(value));
				mainRootElement.appendChild(currentElement);
			} 
			if (smartparam != null && StringUtils.isNotEmpty(smartparam)) {
				Element smartparamTag = doc.createElement(Constants.SE_SMART_PARAM);
				smartparamTag.appendChild(doc.createTextNode(smartparam));
				currentElement.appendChild(smartparamTag);
				mainRootElement.appendChild(currentElement);
			}
		}
	}
}
