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
