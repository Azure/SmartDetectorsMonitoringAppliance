import * as React from 'react';
import { Grid, Col, Row } from 'react-flexbox-grid';
//import { Moment } from 'moment';
import * as moment from 'moment';

import InsightsCardsPanel from './insightsCardsPanel';
import InsightDetailsPanel from './insightDetailsPanel';

import Insight from '../../models/Insight';

import './style.css';
import { ChartType } from '../../models/ChartType';

export default class Insights extends React.Component {
    constructor(props: any) {
        super(props);
    }

    // TODO - temp method
    private getInsights() : Insight[] {
        let list: Insight[] = new Array();
        
        // Temp!! - TODO - delete it
        list = [
            new Insight("PROCESS CPU", "VMPROD123", ChartType.Timeline,
                        "someQuery", "89%", moment('4/14/2017 9:49 PM'), undefined,
                        "mySubscriptionId", "myProdResourceGroup"),
            new Insight("REQUEST PERFORMANCE DEGRADATION", "CONTOSO", ChartType.bars,
                        "someQuery2", "1.46s", moment('4/15/2017 9:49 PM'), moment('4/15/2017 10:49 PM'),
                        "mySubscriptionId2", "myProdResourceGroup2")
        ]

        return list;
    }

    private getInsightsCards(insights: Insight[]) : JSX.Element {
        return (
            <div>
                <InsightsCardsPanel insights={insights} />
            </div>
        );    
    }

    render() {
        let insights = this.getInsights();

        return (
            <div className="insightsContainer">
                <div className="filterRow" />

                <Grid fluid className="insightsPanel">
                    <Row className="insightsPanelContent">
                        <Col xs={4} className="insightsCardsPanel">
                            {this.getInsightsCards(insights)}
                        </Col>

                        <Col xs={8}>
                            <InsightDetailsPanel insight={insights[0]}/>
                        </Col>  
                    </Row>
                </Grid>
            </div>
        );
    }
}