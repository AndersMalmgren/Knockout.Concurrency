/// Requires resharper 6 or greater or use test.htm
/// <reference path="knockout-2.1.0.js"/>
/// <reference path="~/../src/knockout.concurrency.js"/>

Item = function (id, value) {
    this.id = id;
    this.value = ko.observable(value).extend({ concurrency: true });
};

test("When traversing simple view model", function () {
    var viewModel = {
        propOne: ko.observable().extend({ concurrency: true }),
        items: [new Item()]
    };
    var data = {
        propOne: "New value",
        items: [{ value: null}]
    };

    new ko.concurrency.Runner(viewModel).run(viewModel, data);

    equal(viewModel.propOne.concurrency().conflict(), true, "It should have detected concurrency for propOne");
    equal(viewModel.items[0].value.concurrency().conflict(), false, "It should not have detected concurrency for item value");

    equal(viewModel.concurrencyConflicts().length, 1, "It should have published that there is one conflict");
});

test("When traversing complex view model with random sorting on items ", function () {
    var viewModel = {
        propOne: ko.observable().extend({ concurrency: true }),
        items: [new Item(1), new Item(2)]
    };

    var data = {
        propOne: "New value",
        items: [{ id: 2, value: "new value" }, { id: 1, value: null}]
    };

    var mapping = {
        items: {
            key: function (item) { return item.id; }
        }
    };

    new ko.concurrency.Runner().run(viewModel, data, mapping);

    equal(viewModel.items[1].value.concurrency().conflict(), true, "It should have detected concurrency for item value");
});

test("When traversing complex view model and items are deleted / added", function () {
    var viewModel = {
        propOne: ko.observable().extend({ concurrency: true }),
        items: ko.observableArray().extend({ concurrency: true })
    };

    viewModel.items([new Item(1), new Item(2)]);

    var data = {
        propOne: "New value",
        items: [{ id: 2, value: null }, { id: 3, value: "New row"}]
    };

    var mapping = {
        items: {
            key: function (item) { return item.id; },
            create: function (data) { return new Item(data.id, data.value); }
        }
    };

    new ko.concurrency.Runner().run(viewModel, data, mapping);
    equal(viewModel.items().length, 3, "IT should not have removed any items");
    equal(viewModel.items()[0].concurrency().otherValue(), "Deleted", "IT should find added concurrency");
    equal(viewModel.items()[2].concurrency().otherValue(), "Added", "IT should find added concurrency");
});

test("When traversing complex view model and items are added before extending array", function () {
    var viewModel = {
        propOne: ko.observable().extend({ concurrency: true }),
        items: ko.observableArray([new Item(1), new Item(2)]).extend({ concurrency: true })
    };

    var data = {
        propOne: "New value",
        items: [{ id: 2, value: null }]
    };

    var mapping = {
        items: {
            key: function (item) { return item.id; },
            create: function (data) { return new Item(data.id, data.value); }
        }
    };

    new ko.concurrency.Runner().run(viewModel, data, mapping);
    equal(viewModel.items().length, 2, "IT should not have removed any items");
    equal(viewModel.items()[0].concurrency().otherValue(), "Deleted", "IT should find added concurrency");
});

test("When traversing view model and a concurrency observable has a complex type", function () {
    var clientSideListOfProps = [new Item(1, "Test"), new Item(2, "Test2"), new Item(3, "Test3")];

    var viewModel = {
        propOne: ko.observable(clientSideListOfProps[0]).extend({ concurrency: true }),
        propTwo: ko.observable("test").extend({ concurrency: true })
    };

    var data = {
        propOne: new Item(2, "Test2"),
        propTwo: "test"
    };

    var mapping = {
        propOne: {
            comparer: function (item1, item2) {
                return item1.id == item2.id;
            },
            create: function (data) {
                return ko.utils.arrayFirst(clientSideListOfProps, function (item) {
                    return data.id == item.id;
                });
            }
        },
        propTwo: {
        }
    };

    new ko.concurrency.Runner().run(viewModel, data, mapping);
    equal(viewModel.propOne.concurrency().conflict(), true, "IT should find concurrency on a complex type");
    equal(viewModel.propOne.concurrency().otherValue().id, 2, "IT use correct complex type as other value");

    //The KO options binding requries that the value binding and the options binding share the same references
    viewModel.propOne.concurrency().takeServer();
    equal(viewModel.propOne(), clientSideListOfProps[1], "IT should not use reference from data");
});