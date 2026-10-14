// The appeal's script module (T366, flow 09): a reason box's character counter, and the appeal entry's clamped reason.
// Imported by the components that need it (MyReview, AppealEntry, the STAR register) through Blazor's JS interop, from
// this file under 'self': no inline script and no handler attribute, which the CSP forbids.

// The counter (round 3 S9): a reason box's counter, a second .field-help named by its aria-describedby, counts as the
// reason is typed, in the browser, so no keystroke costs a round trip. Blazor renders the same words from what it holds,
// so a re-render agrees with it. Attached once per textarea; a textarea Blazor replaces is attached again. Used by the
// lodge block's reason and the STAR register's revocation reason (T366, D2): each passes its own limit.
const limitFormat = new Intl.NumberFormat("en-US");

export function count(textarea, counter, limit) {
    if (!textarea || !counter || textarea.dataset.counted === "true") {
        return;
    }

    textarea.dataset.counted = "true";
    const update = () => {
        counter.textContent = `${limitFormat.format(textarea.value.length)} of ${limitFormat.format(limit)} characters`;
    };
    textarea.addEventListener("input", update);
    update();
}

// "Show the whole reason" (S10): drawn only when the clamped reason overflows its six lines, measured after render.
export function overflows(element) {
    return !!element && element.scrollHeight > element.clientHeight + 1;
}

// Measured once, a reason that fits at 1280 keeps no button at 390, and a font that arrives late changes the lines (T366,
// D5). Watched while the entry is on the page: the component hears only when the answer changes.
export function watchOverflow(element, component) {
    if (!element || !component || element.__overflowWatch) {
        return;
    }

    let last = overflows(element);
    const check = () => {
        if (!element.__overflowWatch) {
            return;
        }

        const now = overflows(element);
        if (now !== last) {
            last = now;
            component.invokeMethodAsync("OverflowChanged", now).catch(() => { });
        }
    };

    const observer = typeof ResizeObserver === "function" ? new ResizeObserver(check) : null;
    observer?.observe(element);
    element.__overflowWatch = observer ?? true;
    document.fonts?.ready.then(check);
}

export function unwatchOverflow(element) {
    if (element?.__overflowWatch) {
        element.__overflowWatch.disconnect?.();
        delete element.__overflowWatch;
    }
}
