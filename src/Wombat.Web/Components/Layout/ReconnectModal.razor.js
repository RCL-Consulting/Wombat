// The reconnect dialog's script (T335 flow 01, T330; DESIGN.md § The reconnect dialog and the error bar), rewritten from
// the .NET 10 template's.
//
// What blazor.web.js does, read in its 10.0.12 source (UserSpecifiedDisplay, DefaultReconnectionHandler): it finds this
// dialog by its id, and as the connection drops it puts one components-reconnect-* class on it and raises
// components-reconnect-state-changed, whose detail.state is "show", then "retrying" once a second while it counts down to
// its next attempt (detail.secondsToNextAttempt) and once more with 0 as the attempt starts, and at the end "hide",
// "failed" or "rejected"; or "show" and at once "paused" when the circuit is paused. It opens nothing, and with this
// dialog in the page it never reloads by itself: this script does both. ReconnectModal.razor.css shows the one state
// element the classes name; this script keeps the dialog's role, name, description and live region in step with it.

const dialog = document.getElementById("components-reconnect-modal");
const live = dialog.querySelector("[data-reconnect-live]");
const countdown = dialog.querySelector("[data-reconnect-countdown]");
const tryAgain = dialog.querySelector('[data-reconnect-action="retry"]');

// The runtime's classes. This script sets one itself only where the runtime raises nothing: a Resume that cannot reach
// the server, and a reload this script starts.
const runtimeClasses = [
    "components-reconnect-show", "components-reconnect-hide", "components-reconnect-retrying",
    "components-reconnect-failed", "components-reconnect-rejected", "components-reconnect-paused",
    "components-reconnect-resume-failed",
];

let focusWhenSettled = false;
let settleQueued = false;
let announced = "";
let busy = false;

dialog.addEventListener("components-reconnect-state-changed", event => {
    const detail = event.detail;
    switch (detail.state) {
        case "show":
            dialog.removeAttribute("data-attempt");
            break;
        case "hide":
            close();
            return;
        case "retrying": {
            // Picks Could not reconnect's line: the count, or "Trying again now." and the bar. The state stays one element
            // either way, so settle() finds the focus still on its heading and leaves it there.
            const seconds = detail.secondsToNextAttempt;
            dialog.dataset.attempt = seconds > 0 ? "waiting" : "started";
            if (seconds > 0) {
                countdown.textContent = `${seconds} ${seconds === 1 ? "second" : "seconds"}`;
            }
            break;
        }
        case "failed":
            // As the template does: try again by itself when the person comes back to the tab.
            document.addEventListener("visibilitychange", retryWhenVisible);
            break;
        case "rejected":
            // The server was reached, and holds neither the circuit nor a state to resume it from.
            location.reload();
            break;
    }

    open();
});

// It cannot be dismissed: nothing behind it answers until the connection is back.
dialog.addEventListener("cancel", event => event.preventDefault());

dialog.querySelectorAll("[data-reconnect-action]").forEach(button => {
    const action = button.dataset.reconnectAction === "retry" ? retry : resume;
    button.addEventListener("click", () => press(button, action));
});

function open() {
    if (!dialog.open) {
        dialog.showModal();
        focusWhenSettled = true;
    }

    if (!settleQueued) {
        // After this event and any the runtime raises in the same breath ("paused" comes straight after "show").
        settleQueued = true;
        queueMicrotask(settle);
    }
}

function close() {
    document.removeEventListener("visibilitychange", retryWhenVisible);
    focusWhenSettled = false;
    announced = "";
    live.textContent = "";
    if (dialog.open) {
        dialog.close();
    }
}

function settle() {
    settleQueued = false;
    const shown = [...dialog.querySelectorAll("[data-reconnect-state]")].find(state => state.getClientRects().length > 0);
    if (!dialog.open || !shown) {
        return;
    }

    const heading = shown.querySelector("h2");
    dialog.setAttribute("aria-labelledby", heading.id);
    dialog.setAttribute("aria-describedby", shown.querySelector("p[id]").id);
    // An alertdialog where the person has to act (Connection lost, Could not resume); the <dialog>'s own role elsewhere.
    if (shown.dataset.role) {
        dialog.setAttribute("role", shown.dataset.role);
    } else {
        dialog.removeAttribute("role");
    }

    const sentence = shown.dataset.announce;
    if (focusWhenSettled) {
        // As it opens: the heading takes the focus, which reads the dialog's name and description.
        focusWhenSettled = false;
        heading.focus();
        announced = sentence;
        return;
    }

    if (!shown.contains(document.activeElement)) {
        // The state that held the focus has gone (Reconnecting hides as the runtime begins to retry), and a hidden
        // element's focus falls to the body inside the modal, where Connection lost, an alertdialog, would never take it.
        // The shown state's heading takes it. A move inside the dialog reads the heading alone, so the sentence is still
        // said below.
        heading.focus();
    }

    if (sentence !== announced) {
        // A state it has moved to: its sentence, without the count, once.
        announced = sentence;
        live.textContent = sentence;
    }
}

// One attempt at a time. The button says it is busy with aria-disabled rather than disabled, so it keeps the focus
// (DESIGN.md § Button system); if its state has gone when the attempt ends, the focus moves to the button of the state
// that took its place (Resume to Could not resume's Try again).
async function press(button, action) {
    if (busy) {
        return;
    }

    busy = true;
    button.setAttribute("aria-disabled", "true");
    try {
        await action();
    } finally {
        busy = false;
        button.removeAttribute("aria-disabled");
        if (dialog.open && button.getClientRects().length === 0) {
            [...dialog.querySelectorAll("[data-reconnect-action]")].find(other => other.getClientRects().length > 0)?.focus();
        }
    }
}

// Connection lost's Try again. Blazor.reconnect() resolves true once reconnected (the runtime then raises "hide"), false
// when the server answers but no longer holds the circuit, and throws when the server cannot be reached. Then
// Blazor.resumeCircuit() starts a new circuit from the state the server kept when the old one went: true when it could.
async function retry() {
    document.removeEventListener("visibilitychange", retryWhenVisible);
    try {
        if (!await Blazor.reconnect()) {
            if (await Blazor.resumeCircuit()) {
                close();
            } else {
                reload();
            }
        }
    } catch {
        // Not reached: Connection lost stays, and tries again when the tab is next shown.
        document.addEventListener("visibilitychange", retryWhenVisible);
    }
}

// Resume, and Could not resume's Try again. Blazor.resumeCircuit() resolves true once resumed (the runtime then raises
// "hide"), false when the server answers but has nothing to resume, and throws when the server cannot be reached.
async function resume() {
    try {
        if (!await Blazor.resumeCircuit()) {
            reload();
        }
    } catch {
        enter("components-reconnect-resume-failed");
    }
}

function retryWhenVisible() {
    if (document.visibilityState === "visible") {
        press(tryAgain, retry);
    }
}

// The server no longer holds this page: say so while it reloads.
function reload() {
    enter("components-reconnect-rejected");
    location.reload();
}

function enter(stateClass) {
    dialog.classList.remove(...runtimeClasses);
    dialog.classList.add(stateClass);
    open();
}
