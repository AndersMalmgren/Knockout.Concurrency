ko.concurrencyDemo.PuppyViewModel = function (data) {
    this.Id = ko.observable();
    this.Name = ko.observable().extend({ concurrency: true });
      
    return ko.mapping.fromJS(data ? data : {}, {}, this);
};

ko.concurrencyDemo.DogViewModel = function (data) {
    this.sessionId = ko.concurrencyDemo.guid();
    this.Name = ko.observable().extend({ concurrency: true });
    this.Stray = ko.observable().extend({ concurrency: { presenter: function (value) { return value ? "Yes" : "No"; } } });

    this.Puppies = ko.observableArray().extend({ concurrency: true });

    this.concurrencyRunner = new ko.concurrency.Runner(this);
    this.hasConflicts = ko.computed(function () {
        return this.concurrencyConflicts().length > 0;
    }, this);

    signalR.eventAggregator.subscribe(Knockout.Concurrency.Demo.Events.Message.of("Knockout.Concurrency.Demo.Models.Dog"), this.saved, this, { sessionId: this.sessionId });

    return ko.mapping.fromJS(data, ko.concurrencyDemo.DogViewModel.mapping, this);
};

ko.concurrencyDemo.DogViewModel.prototype = {
    addPuppy: function () {
        this.Puppies.push(new ko.concurrencyDemo.PuppyViewModel({ Id: 0, Name: "" }));
    },
    removePuppy: function (item) {
        this.Puppies.remove(item);
    },
    save: function () {
        var data = ko.toJSON({
            Data: ko.mapping.toJS(this),
            SessionId: this.sessionId
        });
        $.ajax({ url: "/Home/Save", data: data, type: "POST", contentType: 'application/json; charset=utf-8', success: $.proxy(function (response) {
            this.Puppies.remove(function (item) { return item.Id() == 0; });
            ko.mapping.fromJS(response, ko.concurrencyDemo.DogViewModel.mapping, this);
        }, this)
        });
    },
    resolveAll: function () {
        this.concurrencyRunner.takeServer();
    },
    saved: function (data) {
        var mappings = {
            Puppies: {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.Id);
                },
                create: function (data) {
                    return new ko.concurrencyDemo.PuppyViewModel(data);
                },
                deleted: function (item) {
                    item.Id(0);
                }
            }
        };
        this.concurrencyRunner.run(this, data.Data, mappings);
    }
};

ko.concurrencyDemo.DogViewModel.mapping = {
    Puppies: {
        key: function (data) {
            return ko.utils.unwrapObservable(data.Id);
        },
        create: function (options) {
            return new ko.concurrencyDemo.PuppyViewModel(options.data);
        }
    }
};

$(document).ready(function () {
    $.getJSON("/Home/GetDog", null, function (data) {
        ko.applyBindings(new ko.concurrencyDemo.DogViewModel(data));
    });
});