import { Moment } from 'moment';

export class DateUtils {
    public static getStartTimeAndEndTimeAsRange(startTime: Moment, endtime?: Moment) : string {
        let result: string = "";

        result += startTime.format("M/D HH:mm A");

        if (endtime) {
            if (endtime.isSame(startTime, 'day')) {
                result += " - " + endtime.format("HH:mm A");
            }
            else {
                result += " - " + endtime.format("M/D HH:mm A");
            }
        }

        return result;
    }
}