# PasswordRules

The six password rules, listed under a new-password field before anything is typed: one heading, "The new password needs:", then one sentence a rule, in the one order every refusal lists them in. Flow 02 (T339, 2026-09-28, C2 and E11) built it; until then a page listed no rule until one was broken, in Identity's own words ("Passwords must have at least one uppercase ('A'-'Z').") and in the order its validator found them.

## What the consumer provides

`<PasswordRules Id="new-password-rules" />`, as the child content of a new-password `PasswordField`, which names the list with `aria-describedby` by that id. On change password, register and the administrator's reset card (a user's page).

## Markup

```html
<div id="new-password-rules" class="password-rules">
  <p>The new password needs:</p>
  <ul>
    <li>At least 12 characters.</li>
    <li>At least 4 different characters.</li>
    <li>A digit (0 to 9).</li>
    <li>An upper-case letter.</li>
    <li>A lower-case letter.</li>
    <li>A symbol, such as ! or #.</li>
  </ul>
</div>
```

`.password-rules`: `muted-text` at 0.9rem; the heading's `p` with no margin; the list 2px under it, indented 1.25rem.

## The words and the order

The sentences are `WombatIdentityErrorDescriber`'s (through `PasswordRuleMessages`), which Identity's own errors carry too, so a page, a refusal and the reset card that reads the errors say the same thing. The order is fixed (`RuleOrder`): length, different characters, digit, upper case, lower case, symbol. The numbers come from the configured `PasswordOptions` (12 and 4 today), never from the page.

## A refusal

- **A refusal lists only the rules broken**, in the same order, under the same heading, inside its danger Alert: `ul.password-rules-broken` (4px under the words, indented 1.25rem). On change password it reads "**Your password was not changed.** The new password needs:" and then the broken rules.
- **The field is marked**: `aria-invalid`, and its own `.validation-message`, "The new password does not meet the rules below." (`PasswordRuleMessages.FieldMessage`).
- **The field names the list until a refusal is shown** (A15); then it names its own message and the refusal, not the list, which still stands under it saying all six.
- The reset card does the same for a refusal that lists rules; any other refusal leaves its field alone. It has no 8-character rule: Reset password is enabled once anything is typed.

## Contrast

`muted-text` on the surface, 5.09:1.
