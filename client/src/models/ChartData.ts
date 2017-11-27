import { Moment } from 'moment';

export default interface IChartData {
    time: Moment[];

    value: number[];
}