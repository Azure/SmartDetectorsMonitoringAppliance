import * as React from 'react';

import Card from '../Card';

import Insight from '../../models/Insight';

interface IInsightsCardsPanelProps {
    insights: Insight[];
}


export default class InsightsCardsPanel extends React.Component<IInsightsCardsPanelProps> {
    constructor(props: IInsightsCardsPanelProps) {
        super(props);

        this.getInsightsCards = this.getInsightsCards.bind(this);
    }

    private getInsightsCards() : JSX.Element[] {
        return this.props.insights.map((insight, index) => (
            <div>
                <Card title={insight.insightName}/>
            </div>
        ));
    }

    render() {
        return (
            <div>
                {this.getInsightsCards()}
            </div>
        );
    }
}