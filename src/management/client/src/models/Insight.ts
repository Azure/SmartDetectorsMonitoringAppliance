import { Moment } from 'moment';

import ChartType from './ChartType';

export default class Insight {
    /**
     * The insight start time timestamp (Mandatory)
     */
    public insightStartTime: Moment;

    /**
     * The insight end time timestamp (Not mandatory)
     */
    public insightEndTime?: Moment;

    /**
     * The insight's subscription id
     */
    public subscriptionId?: string;

    /**
     * The insight's resource group
     */
    public resourceGroup?: string;

    /**
     * The insights's resource name
     */
    public resourceName: string;

    /**
     * The insight name
     */
    public insightName: string;

    /**
     * The insight rule name
     */
    public ruleName: string;

    /**
     * Additional information for this insight
     */
    public additionalInformation?: IDict<string>;

    /**
     * The insight's chart type
     */
    public insightChartType: ChartType;

    /**
     * The insight's query
     */
    public insightChartQuery: string;

    /**
     * The insight's metric value
     */
    public metricValue: string;

    public constructor(insightName: string, resourceName: string, 
                       insightChartType: ChartType, insightChartQuery: string, metricValue: string,
                       insightStartTime: Moment, insightEndTime?: Moment, subscriptionId?: string,
                       resourceGroup?: string, additionalInformation?: IDict<string>) {
        this.insightName = insightName;
        this.insightStartTime = insightStartTime;
        this.insightEndTime = insightEndTime;
        this.resourceName = resourceName;
        this.insightChartType = insightChartType;
        this.insightChartQuery = insightChartQuery;
        this.metricValue = metricValue;
        this.subscriptionId = subscriptionId;
        this.resourceGroup = resourceGroup;
        this.additionalInformation = additionalInformation;
    }
}