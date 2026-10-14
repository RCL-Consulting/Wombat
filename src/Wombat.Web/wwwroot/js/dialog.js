// The native <dialog> behind ConfirmDialog. Opened modal, so Escape closes it. The browser returns the focus to the
// control that opened it, except where a clicked button never took the focus (Safari, Firefox on macOS): so the opener is
// recorded here (the active element, else the caller's id) and focused when Cancel or Escape closes the dialog (T366, D6).
// A close by the caller (a confirm) leaves the focus to the caller, which puts it on its result.
window.wombatDialog = window.wombatDialog || {
  showModal: function (element, returnFocusId) {
    if (element && !element.open) {
      const active = document.activeElement;
      element.__wombatOpener = active && active !== document.body
        ? active
        : (returnFocusId ? document.getElementById(returnFocusId) : null);
      if (!element.__wombatWired) {
        element.__wombatWired = true;
        element.addEventListener("cancel", function () { element.__wombatReturnFocus = true; });
        element.addEventListener("close", function () {
          const opener = element.__wombatOpener;
          const returnFocus = element.__wombatReturnFocus;
          element.__wombatReturnFocus = false;
          element.__wombatOpener = null;
          if (returnFocus && opener && opener.isConnected && typeof opener.focus === "function") {
            opener.focus();
          }
        });
      }

      element.__wombatReturnFocus = false;
      element.showModal();
    }
  },
  close: function (element, returnFocus) {
    if (element && element.open) {
      element.__wombatReturnFocus = !!returnFocus;
      element.close();
    }
  }
};
