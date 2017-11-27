import * as React from 'react';
import { Card as MDCard, CardTitle, CardText } from 'react-md/lib/Cards';

import './style.css';

interface ICardProps {
    title: string;
}

interface ICardState {
}

export default class Card extends React.Component<ICardProps, ICardState> {
    constructor(props: ICardProps) {
        super(props);
    }

    private getHeaderInsightTimestampElement() : JSX.Element {
        // TODO - convert the given timestamp to the required format
        return (
            <div className="insightTimestamp">
                4/14/2017 9:49 PM
            </div>
        );
    }

    render() {
        const { title } = this.props;

        return (
            <div className="cardContainer">
                <MDCard>
                    <div className="cardHeader">
                        <CardTitle title="" subtitle={title} children={this.getHeaderInsightTimestampElement()} />
                    </div>
                    <CardText>
                    <h1>86%</h1>
                    This will contain much better information
                    </CardText>
                </MDCard>
            </div>
        );
    }
}