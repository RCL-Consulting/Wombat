# ErrorPage

The page a failed page load is rerun into: "Something went wrong", what to do, the reference to quote (ReferenceBlock), Try again and Go to Home; typed as an address, "Nothing went wrong".

Flow 01 (T335, b347e11c; T321; R2-Error-*; S5, S6). Source: `Components/Pages/Error.razor`, `/Error`; the handler is `Navigation/ErrorPages.cs`.

## What the consumer provides

Nothing: it is a page. The exception handler reruns a failed page load here (a GET that accepts HTML; a failed post or `fetch` keeps its bare 500), answered 500.

## States (DESIGN.md § System pages)

| State | Heading and icon | What it says | The way on |
|---|---|---|---|
| After a failure, signed in | Something went wrong, `triangle-alert` in `warning-color` | "Wombat could not finish this request. Try again. If it keeps happening, send this reference to your institution's Wombat administrator." ("… to the platform administrator." for an Institutional admin, a College admin or an Administrator; a Speciality or Sub-speciality admin reads "your institution's Wombat administrator"), then the ReferenceBlock | Try again (the failed address, a full load, `refresh-cw`), Go to Home (`.btn-outline`, `home`) |
| After a failure, signed out | the same, in a `.system-card` | "… send this reference to whoever sent you the link, or to your Wombat administrator." (an MSF respondent has no account), then the ReferenceBlock | Try again, Go to Home, with no icons |
| Typed as `/Error` | Nothing went wrong, `info` in `secondary-color` | "This is Wombat's error page, opened directly. No request failed, so there is nothing to report." | Go to Home (primary) |

- With no failed address to go back to, Go to Home is the primary button.
- Signed in, the page is a `.system-panel` in the shell; signed out, a `.system-card` under the signed-out bar. Below 641px the buttons stack at 44px and the reference block goes to one column.

## Rules

- **Static and anonymous.** It renders once, inside the request that failed, so the reference is that request's, and a visitor who has not signed in sees it rather than a sign-in form.
- **It reads no database**, nor does its shell. In a database outage the rerun cannot check the sign-in, so the page is drawn signed out, and the cookie is left as it was.
- **Sign out is a link here**, to `/account/logout-confirm`: a form's token from the failed pass could be refused.
- Tab and heading are the same words: "Something went wrong · Wombat".

## Contrast

The icon: `warning-color` 5.47:1 on the page (5.77:1 in the white signed-out card), `secondary-color` 4.61:1. The reference block's labels 4.57:1 on `header-bg`.
