package com.ibm.ecm.extension.utils;

/**
 * Exceptions that are thrown by the Smart Export Template Builder plugin are wrapped into this class.
 *
 */
public class SETemplateBuilderException extends Exception {
	
	/**
	 * Constructor.
	 * @param message exception message.
	 */
	public SETemplateBuilderException(String message) {
		super("SETemplateBuilder Exception : "+message);
	}
}
