import { HubConnection } from "./HubConnection";
import { IHttpConnectionOptions } from "./IHttpConnectionOptions";
import { IHubProtocol } from "./IHubProtocol";
import { ILogger, LogLevel } from "./ILogger";
import { HttpTransportType } from "./ITransport";
export declare class HubConnectionBuilder {
    configureLogging(logging: LogLevel | ILogger): HubConnectionBuilder;
    withUrl(url: string): HubConnectionBuilder;
    withUrl(url: string, options: IHttpConnectionOptions): HubConnectionBuilder;
    withUrl(url: string, transportType: HttpTransportType): HubConnectionBuilder;
    withHubProtocol(protocol: IHubProtocol): HubConnectionBuilder;
    build(): HubConnection;
}
