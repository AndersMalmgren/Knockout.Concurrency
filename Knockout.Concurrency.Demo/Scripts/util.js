(function () {
    ko.concurrencyDemo = {};

    ko.concurrencyDemo.Polling = function (url, parameters, callback) {
        this.url = url;
        this.parameters = parameters;
        this.callback = callback;
        this.sessionId = guid();
        this.startPolling();
    };

    ko.concurrencyDemo.Polling.prototype = {
        abort: function () {
            if (this.request != null) {
                this.request.abort();
            }
        },
        startPolling: function (xhr) {
            if (xhr != null && xhr.status == 0) return;
            this.parameters.sessionId = this.sessionId;
            this.request = $.getJSON(this.url, this.parameters, $.proxy(this.responseCallback, this));
        },
        responseCallback: function (data) {
            if (!data.Timeout) {
                this.callback(data.Data);
            }
            this.startPolling();
        }
    };

    function S4() {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    }

    function guid() {
        return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
    }
})();