(function () {

    var defaultOptions = {
        containerClass: "concurrency",
        takeLocalButtonTitle: "Take local",
        takeLocalButtonClassName: "take-local",
        takeRemoteButtonTitle: "Take server",
        takeRemoteButtonClass: "take-server",
        itemAddedCaption: "Added",
        itemDeletedCaption: "Deleted",
        autoInjectViewForArrays: true
    };


    //Concurrency API Helpers
    ko.concurrency = (function () {
        return {
            init: function (options) {
                options = options || {};

                defaultOptions = ko.utils.extend(defaultOptions, options);
            },
            bindingOverrides: {
                value: true,
                checked: true
            }
        };
    } ());

    //Internal helpers
    var helpers = (function () {
        return {
            conflictResolvers: {
                itemAdded: {
                    takeServer: function () { },
                    takeLocal: function (listener) {
                        helpers.removeConcurrencyItem(listener);
                    }
                },
                itemDeleted: {
                    takeServer: function (listener) {
                        helpers.removeConcurrencyItem(listener);
                    },
                    takeLocal: function () { }
                },
                value: {
                    takeServer: function (listener) {
                        listener.currentValue(listener.otherValue());
                    },
                    takeLocal: function () { }
                }
            },
            removeConcurrencyItem: function (listener) {
                listener.array.remove(function (item) { return item.concurrency() == listener; });
            },
            extendObservableArray: function (arr, options) {
                arr.concurrencyExtendOptions = options;
                var checkArrayForNewItems = function (value) {
                    ko.utils.arrayForEach(value, function (item) {
                        if (item.concurrency == null) {
                            helpers.concurrencyExtendObservable(item, options, arr);
                        }
                    });
                };

                if (arr().length > 0)
                    checkArrayForNewItems(arr());
                arr.subscribe(checkArrayForNewItems);
            },
            concurrencyExtendObservable: function (observable, options, arr) {
                observable.concurrency = ko.observable(new ko.concurrency.ConcurrencyViewModel(false, null, observable, options, arr));
            },
            registerBindingOverrides: function () {
                for (var name in ko.concurrency.bindingOverrides) {
                    if (ko.concurrency.bindingOverrides[name]) {
                        helpers.registerBindingOverride(name);
                    }
                }

                //Special case for templateBinding
                helpers.overrideTemplateBinding();
            },
            registerBindingOverride: function (name) {
                var binding = ko.bindingHandlers[name];
                var init = binding.init;

                binding.init = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                    if (init) {
                        init(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                    }

                    var value = valueAccessor();
                    if (value.concurrency) {
                        var container = document.createElement('SPAN');
                        element.parentNode.insertBefore(container, element.nextSibling);
                        ko.applyBindingsToNode(container, { concurrency: value.concurrency });
                    }
                };
            },
            renderConcurrencyView: function (container, concurrency) {
                var unwrapped = ko.utils.unwrapObservable(concurrency());
                var options = unwrapped.options;

                var span = document.createElement('SPAN');

                var takeRemoteButton = document.createElement('BUTTON');
                var takeLocalButton = document.createElement('BUTTON');

                ko.utils.registerEventHandler(takeLocalButton, "click", function () {
                    unwrapped.takeLocal();
                });

                ko.utils.registerEventHandler(takeRemoteButton, "click", function () {
                    unwrapped.takeServer();
                });

                container.className = options.containerClass;
                takeLocalButton.title = options.takeLocalButtonTitle;
                takeLocalButton.className = options.takeLocalButtonClassName;
                takeRemoteButton.title = options.takeRemoteButtonTitle;
                takeRemoteButton.className = options.takeRemoteButtonClass;

                container.appendChild(span);
                container.appendChild(takeRemoteButton);
                container.appendChild(takeLocalButton);

                ko.applyBindingsToNode(container, { visible: unwrapped.conflict });
                ko.applyBindingsToNode(span, { concurrencyValue: unwrapped });
            },
            overrideTemplateBinding: function () {
                var foreachBinding = ko.bindingHandlers.foreach;
                var initForeach = foreachBinding.init;

                foreachBinding.init = function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
                    if (valueAccessor().concurrencyExtendOptions) {
                        if (valueAccessor().concurrencyExtendOptions.autoInjectViewForArrays) {
                            var binding = document.createElement('SPAN');
                            binding.setAttribute("data-bind", "concurrency: concurrency");

                            element.appendChild(binding);
                        }
                    }
                    return initForeach(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext);
                };
            },
            extendLiteral: function (target, source) {
                for (var index in source) {
                    if (target[index] == null) {
                        target[index] = source[index];
                    }
                }

                return target;
            }
        };
    } ());

    ko.concurrency.ConcurrencyViewModel = function (conflict, otherValue, currentValue, options, array) {
        this.options = options;
        this.resolved = ko.observable(false);
        this.conflict = ko.observable(conflict);
        this.otherValue = ko.observable(otherValue);
        this.currentValue = currentValue;
        this.array = array;
    };

    ko.concurrency.ConcurrencyViewModel.prototype = {
        resolve: function () {
            this.resolved(true);
            this.conflict(false);
            this.runner.removeConflict(this);
        },
        takeLocal: function () {
            this.resolve();
            this.resolver.takeLocal(this);
        },
        takeServer: function () {
            this.resolve();
            this.resolver.takeServer(this);
        }
    };

    ko.extenders.concurrency = function (target, options) {
        if (options == true) {
            options = {};
        }

        options = helpers.extendLiteral(options, defaultOptions);

        if (target.push) { //Observable array
            helpers.extendObservableArray(target, options);
        } else {
            helpers.concurrencyExtendObservable(target, options);
        }
        return target;
    };

    ko.bindingHandlers.concurrency = {
        init: function (element, valueAccessor) {
            helpers.renderConcurrencyView(element, valueAccessor);
        }
    };

    ko.bindingHandlers.concurrencyValue = {
        update: function (element, valueAccessor) {
            var concurrency = ko.utils.unwrapObservable(valueAccessor());
            var otherValue = concurrency.options.presenter ? function () { return concurrency.options.presenter(concurrency.otherValue()); } : concurrency.otherValue;
            ko.bindingHandlers.text.update(element, otherValue);
        }
    };

    ko.concurrency.Runner = function (model) {
        if (model != null) {
            this.conflicts = ko.observableArray();
            model.concurrencyConflicts = this.conflicts;
        }
    };
    ko.concurrency.Runner.prototype = {
        clear: function () {
            if (this.conflicts) {
                this.conflicts.removeAll();
            }
        },
        run: function (vm1, vm2, mappings) {
            if (vm2 == null) { //VM1 is ko object, while VM2 is a json object with less members
                return false;
            }

            for (var index in vm1) {
                var mapping = mappings ? mappings[index] : null;
                var m2Index = mapping && mapping.name ? mapping.name : index;

                var m1 = vm1[index];
                var m2 = vm2[m2Index];

                if (m1 == null) continue;

                var listener = m1.concurrency;


                if (ko.isObservable(m1) && !m1.push) { //observable
                    var val1 = ko.utils.unwrapObservable(m1);
                    var val2 = ko.utils.unwrapObservable(m2);

                    if (this.isComplex(val1) && listener == null) {
                        this.run(val1, val2, mapping);
                    } else if (listener != null) {
                        this.compareAndNotify(val1, val2, listener, mapping, m1, m2);
                    }
                } else if (m1.push) { //array or observablearray
                    var observableArray = m1;
                    var arr1 = ko.utils.unwrapObservable(m1);
                    var arr2 = ko.utils.unwrapObservable(m2);
                    if (arr2 != null) {
                        if (mapping && mapping.key) {
                            //TODO: find a better search algoritgm that does not have Ordo n^2, the problem is that most good ones need to sort the array.
                            //If we clone the array we get Ordo n*2 for the cloning + whatever complexity the search algorithm has, should be cheaper then doing like now
                            var processed = {};
                            for (var i in arr1) {
                                var found = false;
                                var key = mapping.key(arr1[i]);
                                var item = arr1[i];

                                for (var j in arr2) {
                                    if (key == mapping.key(arr2[j])) {
                                        this.run(item, arr2[j], mapping);
                                        found = true;
                                        break;
                                    }
                                }

                                if (!found && item.concurrency) {
                                    this.itemDeleted(item, mapping);
                                }

                                processed[mapping.key(arr1[i])] = true;
                            }

                            for (var i in arr2) {
                                item = arr2[i];
                                if (!processed[mapping.key(item)] && observableArray.concurrencyExtendOptions) {
                                    this.itemAdded(observableArray, item, mapping);
                                }
                            }
                        } else {
                            for (var i in arr1) {
                                if (arr2[i] != null) {
                                    this.run(arr1[i], arr2[i], mapping);
                                }
                            }
                        }
                    }
                } else if (this.isComplex(m1) && m1.$constructor == null) {
                    this.run(m1, m2, mapping);
                } else if (listener != null) {
                    this.compareAndNotify(m1, m2, listener, mapping);
                }
            };
        },
        takeServer: function () {
            var arr = ko.utils.unwrapObservable(this.conflicts);
            while (arr.length > 0) {
                arr[0].takeServer();
            }
        },
        isComplex: function (val) {
            return val instanceof Object;
        },
        itemAdded: function (array, data, mapping) {
            var options = array.concurrencyExtendOptions ? array.concurrencyExtendOptions : defaultOptions;
            var newitem = mapping.create(data);
            array.push(newitem);

            this.notify(true, options.itemAddedCaption, helpers.conflictResolvers.itemAdded, newitem.concurrency);
        },
        itemDeleted: function (item, mapping) {
            if (mapping.deleted) {
                mapping.deleted(item);
            }

            this.notify(true, item.concurrency().options.itemDeletedCaption, helpers.conflictResolvers.itemDeleted, item.concurrency);
        },
        compareAndNotify: function (val1, val2, listener, mapping, m1, m2) {
            val1 = this.getConverterdValue(val1, m1, mapping);
            val2 = this.getConverterdValue(val2, m2, mapping);
            var conflict = val1 != val2;
            this.notify(conflict, val2, helpers.conflictResolvers.value, listener);
        },
        getConverterdValue: function (val, observable, mapping) {
            return mapping && mapping.converter ? mapping.converter(val, observable) : val;
        },
        notify: function (conflict, val2, resolver, listener) {
            var unwrapped = listener();
            unwrapped.conflict(conflict);
            unwrapped.resolved(!conflict);
            unwrapped.otherValue(val2);
            unwrapped.runner = this;
            unwrapped.resolver = resolver;

            this.updateConflictList(unwrapped);
        },
        updateConflictList: function (conflict) {
            if (this.conflicts != null) {
                this.conflicts.remove(conflict);
                if (conflict.conflict()) {
                    this.conflicts.push(conflict);
                }
            }
        },
        removeConflict: function (conflict) {
            if (this.conflicts != null) {
                this.conflicts.remove(conflict);
            }
        }
    };

    var configured = false;
    var applyBindings = ko.applyBindings;
    ko.applyBindings = function (model, node) {
        if (!configured) {
            helpers.registerBindingOverrides();
            configured = true;
        }
        applyBindings(model, node);
    };
} ());