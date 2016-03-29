var historyManager = {
  dataCallback: function () {
    return null;
  },
  titleCallback: function (data) {
    return document.title;
  },
  urlCallback: function (data) {
    return '';
  },
  init: function (config) {
    if (typeof config.data === 'function') {
      this.dataCallback = config.data;
    }
    if (typeof config.title === 'function') {
      this.titleCallback = config.title;
    }
    if (typeof config.url === 'function') {
      this.urlCallback = config.url;
    }
    $(window).bind('popstate', function (e) {
      var newState = e.originalEvent.state;
      if (newState !== null && typeof config.change === 'function') {
        document.title = historyManager.titleCallback(newState);
        config.change(newState);
      }
    });

    var data = this.dataCallback();
    document.title = this.titleCallback(data);
    history.replaceState(data, document.title, this.urlCallback(data));
  },
  setState: function () {
    var data = this.dataCallback();
    history.pushState(data, document.title, this.urlCallback(data));
    document.title = this.titleCallback(data);
  },
};
