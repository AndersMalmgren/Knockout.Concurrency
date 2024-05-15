ko.concurrencyDemo.PuppyViewModel = function (data) {
    this.id = ko.observable();
    this.name = ko.observable().extend({ concurrency: true });
      
    return ko.mapping.fromJS(data ? data : {}, {}, this);
};

ko.concurrencyDemo.DogViewModel = function (data) {
    this.sessionId = ko.concurrencyDemo.guid();
    this.name = ko.observable().extend({ concurrency: true });
    this.stray = ko.observable().extend({ concurrency: { presenter: function (value) { return value ? "Yes" : "No"; } } });

    this.puppies = ko.observableArray().extend({ concurrency: true });

    this.concurrencyRunner = new ko.concurrency.Runner(this);
    this.hasConflicts = ko.computed(function () {
        return this.concurrencyConflicts().length > 0;
    }, this);

    signalR.eventAggregator.subscribe(Knockout.ConcurrencyDemoCore.Events.Message.of("Knockout.ConcurrencyDemoCore.Models.Dog"), this.saved, this, { sessionId: this.sessionId });

    return ko.mapping.fromJS(data, ko.concurrencyDemo.DogViewModel.mapping, this);
};

ko.concurrencyDemo.DogViewModel.prototype = {
    addPuppy: function () {
        this.puppies.push(new ko.concurrencyDemo.PuppyViewModel({ id: 0, name: "" }));
    },
    removePuppy: function (item) {
        this.puppies.remove(item);
    },
    save: async function () {
        var data = ko.toJSON({
            data: ko.mapping.toJS(this),
            sessionId: this.sessionId
        });

        const settings = {
            method: 'POST',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            },
            body: data
        };

        const reponse = await fetch("/api/service/save", settings)
        const json = await reponse.json();
        this.puppies.remove(function (item) { return item.id() == 0; });
        ko.mapping.fromJS(json, ko.concurrencyDemo.DogViewModel.mapping, this);
    },
    resolveAll: function () {
        this.concurrencyRunner.takeServer();
    },
    saved: function (data) {
        var mappings = {
            puppies: {
                key: function (data) {
                    return ko.utils.unwrapObservable(data.id);
                },
                create: function (data) {
                    return new ko.concurrencyDemo.PuppyViewModel(data);
                },
                deleted: function (item) {
                    item.id(0);
                }
            }
        };
        this.concurrencyRunner.run(this, data.data, mappings);
    }
};

ko.concurrencyDemo.DogViewModel.mapping = {
    puppies: {
        key: function (data) {
            return ko.utils.unwrapObservable(data.id);
        },
        create: function (options) {
            return new ko.concurrencyDemo.PuppyViewModel(options.data);
        }
    }
};


(async () => {
    const reponse = await fetch("/api/service");
    ko.applyBindings(new ko.concurrencyDemo.DogViewModel(await reponse.json()), document.getElementById("app"));
})();
