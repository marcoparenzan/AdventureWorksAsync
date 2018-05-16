export interface IStreamSubscriber<T> {
    closed?: boolean;
    next(value: T): void;
    error(err: any): void;
    complete(): void;
}
export interface IStreamResult<T> {
    subscribe(observer: IStreamSubscriber<T>): ISubscription<T>;
}
export interface ISubscription<T> {
    dispose(): void;
}
