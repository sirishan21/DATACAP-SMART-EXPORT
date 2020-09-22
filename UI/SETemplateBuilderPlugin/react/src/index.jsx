import React from 'react';
import ReactDOM from 'react-dom';
import {IntlProvider} from 'react-intl';
import { Provider } from 'react-redux';
import {getNLSResources,getLocale} from './services/NLSService';
import store from './store';
import { ReactLoader } from "./services/ReactLoader"
import { DojoLoader } from "./services/DojoLoader"
import { GlobalMessage} from './services/GlobalMessage'
import SETemplateBuilderFeature from './components/SETemplateBuilderFeature'

if(!window.icnReactLoader || !window.icnReactService){
    window.icnReactLoader = new ReactLoader();
    window.icnDojoLoader = new DojoLoader();
}

const renderSETemplateBuilderFeature = (pl)=> {
    const locale = getLocale(navigator.language);
    const message = getNLSResources(locale);
    ReactDOM.render(
        <IntlProvider locale={locale} messages={message}>
            <Provider store={store}>
                <SETemplateBuilderFeature  />
            </Provider>
        </IntlProvider>,
        pl.containerId
    );
}
const removeSETemplateBuilderFeature =(pl)=> {
    if(pl.containerId){
        ReactDOM.unmountComponentAtNode(pl.containerId);
    }
}

icnReactLoader.registerRender(GlobalMessage.RENDER_SETEMPLATEBUILDERFEATURE, renderSETemplateBuilderFeature);
icnReactLoader.registerRender(GlobalMessage.REMOVE_SETEMPLATEBUILDERFEATURE, removeSETemplateBuilderFeature);
