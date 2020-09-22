package com.ibm.ecm.extension.utils;

import java.text.MessageFormat;
import java.util.Locale;
import java.util.MissingResourceException;
import java.util.ResourceBundle;

public class MessageResources {
	private static final String BUNDLE_NAME = "com.ibm.ecm.extension.nls.ServicesMessages";

	private static ResourceBundle resourceBundle;

	private MessageResources() {
	}

	public static String getMessage(Locale locale, String key) {
		try {
				resourceBundle = ResourceBundle.getBundle(BUNDLE_NAME, locale);
				return resourceBundle.getString(key);
		} catch (MissingResourceException e) {
			try {
				Locale defaultLocale = new Locale("en");
				resourceBundle = ResourceBundle.getBundle(BUNDLE_NAME, defaultLocale);
				return resourceBundle.getString(key);
			} catch (MissingResourceException ex) {
				return key;
			}
		}
	}

	public static String getMessage(Locale locale, String key, Object[] inserts) {
		return MessageFormat.format(getMessage(locale, key), inserts);
	}
}
