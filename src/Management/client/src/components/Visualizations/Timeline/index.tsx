import * as React from 'react';
import * as moment from 'moment';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts'

import FormatUtils from '../../../utils/FormatUtils';

interface ITimelineProps {
    data: object[];
    className?: string;
}

export default class Timeline extends React.Component<ITimelineProps> {
    constructor(props: ITimelineProps) {
        super(props);
    }

    private hourFormat(time: string) {
        return moment(time).format('HH:mm');
    }

    private createLineElements(data: object[]) : JSX.Element[] {
        // 1. Calculates which fields we should create lines for
        let fields: string[] = [];
        for (var key in data[0]) {
            // Ignore 'time' and 'number' fields
            if (key != 'time' && key != 'number') {
                fields.push(key);
            }
        }

        // 2. Create the lines
        var lineElements: JSX.Element[] = [];
        if (fields && fields.length > 0) {
          lineElements = fields.map((line, idx) => {
            return (
              <Line
                stroke="#ff7300"
                strokeWidth={9}
                key={idx}
                type="monotone"
                dataKey={line}
                dot={false}
              />
            );
          });
        }

        return lineElements;
    }

    render() {
        var { data } = this.props;

        return (
            <ResponsiveContainer width="90%" height={300}>
                <LineChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 5 }} className={this.props.className}>
                    <XAxis dataKey="time" tickFormatter={this.hourFormat} minTickGap={20} />
                    <YAxis dataKey="number" type="number" tickFormatter={FormatUtils.kmNumber} />
                    <CartesianGrid strokeDasharray="3 3" />
                    <Tooltip />
                    <Legend />
                    <Line dataKey="number" key="timeValue" type="monotone" strokeWidth={2} dot={false} />
                    {this.createLineElements(this.props.data)}
                </LineChart>
            </ResponsiveContainer>
        );
    }
}