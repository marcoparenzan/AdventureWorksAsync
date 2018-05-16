// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var NullLogger = /** @class */ (function () {
    function NullLogger() {
    }
    NullLogger.prototype.log = function (logLevel, message) {
    };
    NullLogger.instance = new NullLogger();
    return NullLogger;
}());
export { NullLogger };
//# sourceMappingURL=Loggers.js.map