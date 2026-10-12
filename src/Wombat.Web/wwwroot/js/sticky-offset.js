// The chair's stuck sitting bar, measured into --sitting-bar-h on <html> (T361, flow 07; round 2 review 15, round 3 item 15).
// app.css reserves calc(56px + var(--sitting-bar-h, 0px)) as html's scroll-padding-top from 641px, so a heading or result
// that takes the focus, or a #snap-{id} the page is opened at, never lands under the fixed top bar or the bar stuck below
// it (WCAG 2.4.11). Only the chair's bar sticks (.sitting-bar:not(.sitting-bar--reader)); a reader's bar scrolls away, so
// it reserves nothing, and below 641px no bar is fixed. Loaded as a file, no inline handler, which the CSP forbids; it
// writes a custom property through the CSSOM, which the CSP's style rules do not govern.
(function () {
    "use strict";

    var selector = ".sitting-bar:not(.sitting-bar--reader)";
    var wide = window.matchMedia("(min-width: 641px)");
    var observed = null;
    var resize = typeof ResizeObserver === "function" ? new ResizeObserver(measure) : null;
    var pending = false;

    function measure() {
        var height = observed && wide.matches ? Math.ceil(observed.getBoundingClientRect().height) : 0;
        var value = height + "px";
        var root = document.documentElement;
        if (root.style.getPropertyValue("--sitting-bar-h") !== value) {
            root.style.setProperty("--sitting-bar-h", value);
        }
    }

    // The page re-renders in place (Blazor), so the bar can come, go or be replaced at any time: follow whichever one is
    // in the document now.
    function follow() {
        pending = false;
        var bar = document.querySelector(selector);
        if (bar !== observed) {
            if (resize && observed) {
                resize.unobserve(observed);
            }

            observed = bar;
            if (resize && observed) {
                resize.observe(observed);
            }
        }

        measure();
    }

    function schedule() {
        if (!pending) {
            pending = true;
            window.requestAnimationFrame(follow);
        }
    }

    function start() {
        new MutationObserver(schedule).observe(document.body, { childList: true, subtree: true, attributes: true, attributeFilter: ["class"] });
        if (typeof wide.addEventListener === "function") {
            wide.addEventListener("change", schedule);
        }

        window.addEventListener("resize", schedule);
        follow();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", start);
    } else {
        start();
    }
})();
