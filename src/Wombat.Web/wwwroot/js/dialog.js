window.wombatDialog = window.wombatDialog || {
  showModal: function (element) {
    if (element && !element.open) {
      element.showModal();
    }
  },
  close: function (element) {
    if (element && element.open) {
      element.close();
    }
  }
};
