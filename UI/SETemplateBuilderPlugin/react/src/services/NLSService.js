import { IntlProvider, addLocaleData } from 'react-intl';

import intl from 'intl';

let messages = {};
messages["en"] = require('../nls/messages').default
// const supportedLanguages =["ar","bg","ca","cs","da","de","el","en","es","fi","fr","he","hr","hu","it","iw","ja","kk","ko","nb-no","nl","no","pl","pt","pt-br","ro","ru","sk","sl","sv","th","tr","vi","zh","zh-tw"];
const supportedLanguages =["en"]
supportedLanguages.forEach(function(language){
    let nlsName = language.toLowerCase()
    let localeName = language.split("-")[0]
    messages[language]= require('../nls/'+nlsName+'/messages').default
    addLocaleData(require('react-intl/locale-data/'+localeName))
})


export const getNLSResources = (locale)=>{
    locale = locale.toLowerCase()
    if(messages[locale]){
        return messages[locale];
    }else{
        let language =locale.split("-")[0];
        if(messages[language]){
             addLocaleData(require('react-intl/locale-data/'+language))
            return messages[language];
        }else{
            addLocaleData(require('react-intl/locale-data/'+language))
            return messages["en"]; //default English
        }
    }
    
}

export const getLocale = () =>{
    let locale ="en";
    //Load locale from ICN locale settings
    if(document.cookie){
        let settings = document.cookie.split(";")
        for(let i=0;i<settings.length;i++){
            let key = settings[i].split("=")[0]
            if(key.trim()=="icn_locale_language"){
                locale=settings[i].split("=")[1]
                return locale
            }
        }
    }
    //Then browser locale settings
    if(navigator.languages && navigator.languages.length>0){
        locale = navigator.languages[0];
    }else if(navigator.language){
        locale = navigator.language;
    }
    return locale;
}