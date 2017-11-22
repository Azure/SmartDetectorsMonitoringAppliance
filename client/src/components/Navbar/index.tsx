import * as React from 'react';

import { withRouter } from 'react-router';
import { Switch, Route, Link } from 'react-router-dom';

import NavigationDrawer from 'react-md/lib/NavigationDrawers';
import ListItem from 'react-md/lib/Lists/ListItem';
import FontIcon from 'react-md/lib/FontIcons';

import InsightsPage from '../../pages/Insights';

import './index.css';

const TO_PREFIX = '/';

const navItems = [{
    label: 'Inbox',
    to: TO_PREFIX,
    exact: true,
    icon: 'inbox',
  }, {
    label: 'Starred',
    to: `${TO_PREFIX}/starred`,
    icon: 'star',
  }, {
    label: 'Send mail',
    to: `${TO_PREFIX}/send-mail`,
    icon: 'send',
  }, {
    label: 'Drafts',
    to: `${TO_PREFIX}/drafts`,
    icon: 'drafts',
  }];

  const navigationItems = [
    (
      <ListItem
        key={1001}
        component={Link}
        to='/'
        leftIcon={<FontIcon>{'lightbulb_outline'}</FontIcon>}
        tileClassName="md-list-tile--mini"
        primaryText={name || 'Dashboard'}
      />
    ),
    (
        <ListItem
        key={1002}
        component={Link}
        to='something2'
        leftIcon={<FontIcon>{'settings'}</FontIcon>}
        tileClassName="md-list-tile--mini"
        primaryText={name || 'Dashboard'}
      />
    )
  ];

export class Navbar extends React.PureComponent {
    constructor(props: any) {
        super(props);
    }

    render() {
        return (
            <NavigationDrawer
            toolbarTitle="Azure Smart Alerts"
            mobileDrawerType={NavigationDrawer.DrawerTypes.TEMPORARY_MINI}
            tabletDrawerType={NavigationDrawer.DrawerTypes.PERSISTENT_MINI}
            desktopDrawerType={NavigationDrawer.DrawerTypes.PERSISTENT_MINI}
            navItems={navigationItems}
            contentId="main-demo-content"
            drawerClassName="backgroundColor"
            >
                <Switch>
                    <Route path={navItems[0].to} exact component={InsightsPage} />
                    <Route path={navItems[1].to} component={InsightsPage} />
                    <Route path={navItems[2].to} component={InsightsPage} />
                    <Route path={navItems[3].to} component={InsightsPage} />
                </Switch>
        </NavigationDrawer>
        );
    }
}

export default withRouter(Navbar);