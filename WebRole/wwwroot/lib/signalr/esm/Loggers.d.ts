import { ILogger, LogLevel } from "./ILogger";
export declare class NullLogger implements ILogger {
    static instance: ILogger;
    private constructor();
    log(logLevel: LogLevel, message: string): void;
}
