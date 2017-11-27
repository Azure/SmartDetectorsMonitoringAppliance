import * as React from 'react';
import { Grid, Col, Row } from 'react-flexbox-grid';

import { DateUtils } from '../../utils/DateUtils';
import Insight from '../../models/Insight';

import './insightDetailsStyle.css';

interface IInsightDetailsProps {
    insight: Insight;
}

export default class InsightDetails extends React.PureComponent<IInsightDetailsProps> {
    constructor(props: IInsightDetailsProps) {
        super(props);
    }

    render() {
        return (
            <Grid fluid className="insightDetailsContainer">
                <Row className="insightTitle">
                    {this.props.insight.insightName}
                </Row>
                
                <Row className="insightSummaryTitle">
                        Summary
                </Row>

                <Grid className="insightSummaryDetails">
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
            </Grid>
        );
    }
}