import * as React from 'react';

import Insights from '../components/Insights';

interface IInsightsProps {

}

interface IInsightsState {

}

export default class InsightsPage extends React.Component<IInsightsProps, IInsightsState> {
    constructor(props: IInsightsProps) {
        super(props);
    }

    render() {
        return (
            <Insights/>
        );
    }
}