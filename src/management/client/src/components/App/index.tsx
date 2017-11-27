import * as React from 'react';

import Navbar from '../Navbar';

class App extends React.Component<any, any> {

  render() {
    var { children } = this.props;

    return (
      <div>
        <Navbar>
          {children}
        </Navbar>
      </div>
    );
  }
}

export default App;