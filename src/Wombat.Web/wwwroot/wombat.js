window.wombat = window.wombat || {};

window.wombat.clearInvitationTokenFromUrl = function () {
    const url = new URL(window.location.href);
    if (!url.searchParams.has("token")) {
        return;
    }

    url.searchParams.delete("token");
    const search = url.searchParams.toString();
    const next = `${url.pathname}${search ? `?${search}` : ""}${url.hash}`;
    window.history.replaceState({}, "", next);
};

window.wombat.togglePasswordVisibility = function (elementId, visible) {
    const input = document.getElementById(elementId);
    if (!input) {
        return;
    }

    input.type = visible ? "text" : "password";
};

// A server-rendered form marked data-submit-once posts once: a second submit while the first is on its way is dropped,
// and the submit button is disabled once the post has left. The MSF respondent page (T205) is the first. Its link takes
// one response, so a double-click posted the answers twice, and the second post's answer, "this link has already been
// used", replaced the thanks for the first.
document.addEventListener("submit", function (event) {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.hasAttribute("data-submit-once")) {
        return;
    }

    if (form.dataset.submitting === "true") {
        event.preventDefault();
        return;
    }

    form.dataset.submitting = "true";

    // After this event, so the post it starts is not cancelled by the button it was sent from going disabled.
    window.setTimeout(function () {
        form.querySelectorAll("button[type=submit]").forEach(function (button) {
            button.disabled = true;
        });
    }, 0);
});

// A page restored from the back-forward cache comes back as it was left, mid-submit. Let it be sent again.
window.addEventListener("pageshow", function (event) {
    if (!event.persisted) {
        return;
    }

    document.querySelectorAll("form[data-submit-once]").forEach(function (form) {
        delete form.dataset.submitting;
        form.querySelectorAll("button[type=submit]").forEach(function (button) {
            button.disabled = false;
        });
    });
});
