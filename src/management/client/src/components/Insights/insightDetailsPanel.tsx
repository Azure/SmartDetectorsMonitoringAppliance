import * as React from 'react';

import Insight from '../../models/Insight';
import InsightDetails from './insightDetails';

import './insightDetailsPanelStyle.css';

interface IInsightDetailsPanelProps {
    insight?: Insight;
}

export default class InsightDetailsPanel extends React.Component<IInsightDetailsPanelProps> {
    constructor(props: IInsightDetailsPanelProps) {
        super(props);
    }

    render() {
        return (
            <div className="insightDetailsPanelContainer">
                {
                    !this.props.insight &&
                    <div className="noInsightToShow"> 
                        Select an issue to see more details
                    </div>
                }
                {
                    this.props.insight && 
                    <InsightDetails insight={this.props.insight} />
                }
            </div>
        );
    }
}