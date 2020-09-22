/*
 * Licensed Materials - Property of IBM
 * (C) Copyright IBM Corp. 2010, 2019
 * US Government Users Restricted Rights - Use, duplication or disclosure restricted by GSA ADP Schedule Contract with IBM Corp.
 */
import { applyMiddleware, createStore } from 'redux';

import { createLogger } from 'redux-logger'
import thunk from 'redux-thunk';
import { createPromise } from 'redux-promise-middleware'
// import { routerMiddleware } from 'react-router-redux';

import reducers from './reducers';

// const historyMiddleware = routerMiddleware(history);
const promise = createPromise({ types: { fulfilled: 'success' } });
const logger = createLogger();
const middleware = applyMiddleware(promise, thunk, logger);

export default createStore(reducers, middleware);
