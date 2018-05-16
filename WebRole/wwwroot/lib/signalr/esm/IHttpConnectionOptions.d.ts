import { HttpClient } from "./HttpClient";
import { ILogger, LogLevel } from "./ILogger";
import { HttpTransportType, ITransport } from "./ITransport";
export interface IHttpConnectionOptions {
    httpClient?: HttpClient;
    transport?: HttpTransportType | ITransport;
    logger?: ILogger | LogLevel;
    accessTokenFactory?: () => string | Promise<string>;
    logMessageContent?: boolean;
    skipNegotiation?: boolean;
}
