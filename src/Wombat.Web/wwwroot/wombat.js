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

// A password field's Show toggle on a page with no circuit (PasswordField.razor; T339, flow 02, the round 2 review's A3
// to A5). The button is rendered hidden with data-password-toggle, so a page whose script is blocked has no dead button;
// this shows it as the page loads and after an enhanced navigation. One delegated listener drives every toggle: pressed,
// the field it controls (aria-controls) shows its text and aria-pressed says so; the stylesheet swaps the icon. No inline
// handler, which the CSP forbids.
window.wombat.revealPasswordToggles = function () {
    document.querySelectorAll("[data-password-toggle][hidden]").forEach(function (button) {
        button.hidden = false;
    });
};

document.addEventListener("click", function (event) {
    const button = event.target instanceof Element ? event.target.closest("[data-password-toggle]") : null;
    if (!button) {
        return;
    }

    const input = document.getElementById(button.getAttribute("aria-controls"));
    if (!(input instanceof HTMLInputElement)) {
        return;
    }

    const show = button.getAttribute("aria-pressed") !== "true";
    input.type = show ? "text" : "password";
    button.setAttribute("aria-pressed", show ? "true" : "false");
});

// Every field a toggle controls goes back to a password: as its form is posted, so a shown password is not saved in the
// browser's form history as text, and when the page comes back from the back-forward cache, so it is not left on screen.
window.wombat.hidePasswords = function (root) {
    root.querySelectorAll(".password-toggle[aria-controls]").forEach(function (button) {
        const input = document.getElementById(button.getAttribute("aria-controls"));
        if (input instanceof HTMLInputElement) {
            input.type = "password";
        }

        button.setAttribute("aria-pressed", "false");
    });
};

document.addEventListener("submit", function (event) {
    if (event.target instanceof HTMLFormElement) {
        window.wombat.hidePasswords(event.target);
    }
});

window.addEventListener("pageshow", function (event) {
    if (event.persisted) {
        window.wombat.hidePasswords(document);
    }
});

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

// The navigation's lit item, scrolled into view inside the sidebar's list (T335, flow 01; R2-Sidebar-Scroll): the
// Administrator's list is longer than a laptop's window. Only the list scrolls, never the page, and the item keeps 8px
// of the list around it. As a page loads, after an enhanced navigation, when the phone menu opens, and from NavMenu after
// the circuit's first render, which replaces the prerendered list.
window.wombat.revealCurrentNavItem = function () {
    const item = document.querySelector(".nav-list [aria-current]");
    const list = item ? item.closest(".nav-list") : null;
    if (!list || list.clientHeight === 0) {
        return;
    }

    const margin = 8;
    const itemBox = item.getBoundingClientRect();
    const listBox = list.getBoundingClientRect();
    if (itemBox.top < listBox.top + margin) {
        list.scrollTop -= listBox.top + margin - itemBox.top;
    } else if (itemBox.bottom > listBox.bottom - margin) {
        list.scrollTop += itemBox.bottom - (listBox.bottom - margin);
    }
};

document.addEventListener("DOMContentLoaded", function () {
    window.wombat.revealCurrentNavItem();
    window.wombat.revealPasswordToggles();
    if (window.Blazor && typeof window.Blazor.addEventListener === "function") {
        window.Blazor.addEventListener("enhancedload", window.wombat.revealCurrentNavItem);
        window.Blazor.addEventListener("enhancedload", window.wombat.revealPasswordToggles);
    }
});

document.addEventListener("change", function (event) {
    if (event.target instanceof HTMLInputElement && event.target.id === "nav-toggle" && event.target.checked) {
        window.wombat.revealCurrentNavItem();
    }
});

// Moves the focus to the element with this id (T342, flow 03; the build review's A1). A refusal summary's links name
// their field's input by fragment (#observed_on-in), but the page's <base href="/"> resolves a bare fragment against the
// site's root, so a followed link navigated to Home and lost what was typed. The link's handler prevents that and calls
// this instead; the href stays for a page with no circuit. An element that cannot take the focus of itself (a fieldset,
// a read-out) is made programmatically focusable first, as Blazor's FocusOnNavigate does, and brought into view.
window.wombat.focusElement = function (element) {
    if (!(element instanceof HTMLElement)) {
        return false;
    }

    if (!element.hasAttribute("tabindex") && !element.matches("a[href], button, input, select, textarea, summary")) {
        element.setAttribute("tabindex", "-1");
    }

    element.focus();
    return document.activeElement === element;
};

window.wombat.focusById = function (id) {
    return window.wombat.focusElement(document.getElementById(id));
};

// The page's h1, after a load that did not change the page (T342, A3, A4): Log an activity's ?type= changes, and the
// activity page's Try again. FocusOnNavigate moves the focus only when the page itself changes.
window.wombat.focusHeading = function () {
    return window.wombat.focusElement(document.querySelector("h1"));
};
