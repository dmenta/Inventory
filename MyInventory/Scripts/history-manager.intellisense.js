intellisense.annotate(window, {
  // Manage states for pages with ajax changing states.
  historyManager: undefined
});
intellisense.annotate(historyManager, {
  'init': function () {
    /// <signature>
    ///   <summary>Initializes the history manager. Ensure you call it after all initialization has been done.</summary>
    ///   <param name="config" type="historyManagerSettings">A setting of all needed function: data, title, url, change.</param>
    /// </signature>
  },
  'setState': function () {
    ///   <summary>Set a new state for the navigation history</summary>
  },
});
intellisense.annotate(window, {
  // Manage states for pages with ajax changing states.
  historyManager: undefined
});