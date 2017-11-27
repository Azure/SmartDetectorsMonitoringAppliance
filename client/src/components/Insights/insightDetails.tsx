import * as React from 'react';
import { Grid, Col, Row } from 'react-flexbox-grid';

import { DateUtils } from '../../utils/DateUtils';
import Insight from '../../models/Insight';
import Timeline from '../Visualizations/Timeline';

import './insightDetailsStyle.css';

interface IInsightDetailsProps {
    insight: Insight;
}

const data03 = [
    { time: 'Jan 04 2016', number: 105.35 },
    { time: 'Jan 05 2016', number: 102.71 },
    { time: 'Jan 06 2016', number: 100.7 },
    { time: 'Jan 07 2016', number: 96.45 },
    { time: 'Jan 08 2016', number: 96.96 },
    { time: 'Jan 11 2016', number: 98.53 },
    { time: 'Jan 12 2016', number: 99.96 },
    { time: 'Jan 13 2016', number: 97.39 },
    { time: 'Jan 14 2016', number: 99.52 }
];

export default class InsightDetails extends React.PureComponent<IInsightDetailsProps> {
    constructor(props: IInsightDetailsProps) {
        super(props);
    }

    render() {
        return (
            <div className="insightDetailsContainer">
                <Grid fluid className="insightDetailsGrid">
                    <Row className="insightTitle">
                        {this.props.insight.insightName}
                    </Row>
                    
                    <Row className="insightSummaryTitle">
                            Summary
                    </Row>

                    <Grid fluid className="insightSummaryDetails">
                        <Row className="propertyRow">
                            <Col xs={3}>
                                Subscription
                            </Col>
                            <Col xs={3}>
                                {!this.props.insight.subscriptionId ? "N/A" : this.props.insight.subscriptionId}
                            </Col>
                        </Row>
                        <Row className="propertyRow">
                            <Col xs={3}>
                                Resource group
                            </Col>
                            <Col xs={3}>
                                {!this.props.insight.resourceGroup ? "N/A" : this.props.insight.resourceGroup}
                            </Col>
                        </Row>
                        <Row className="propertyRow">
                            <Col xs={3}>
                                Resource
                            </Col>
                            <Col xs={3}>
                                {this.props.insight.resourceName}
                            </Col>
                        </Row>
                        <Row className="propertyRow">
                            <Col xs={3}>
                                Rule name
                            </Col>
                            <Col xs={3}>
                                {this.props.insight.ruleName}
                            </Col>
                        </Row>
                        <Row className="propertyRow">
                            <Col xs={3}>
                                When
                            </Col>
                            <Col xs={3}>
                                {DateUtils.getStartTimeAndEndTimeAsRange(this.props.insight.insightStartTime,
                                                                        this.props.insight.insightEndTime)}
                            </Col>
                        </Row>
                        <Row className="propertyRow">
                            <Col xs={3}>
                                Metric
                            </Col>
                            <Col xs={3}>
                                second thing
                            </Col>
                        </Row>
                    </Grid>

                    <Row className="insightSummaryTitle">
                            Analysis
                    </Row>
                </Grid>
                
                <Grid fluid className="insightAnalysisChart">
                    <Row>
                        <Timeline data={data03} className="analysisChart"/>
                    </Row>
                </Grid>
            </div>
        );
    }
}