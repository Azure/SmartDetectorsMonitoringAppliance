import * as React from 'react';
import { Route, Switch } from 'react-router-dom';

import App from './components/App';
import Insights from './pages/Insights';

export default (
  <Switch>
    <Route component={App} >
      <Route component={Insights} />
    </Route>
  </Switch>
);