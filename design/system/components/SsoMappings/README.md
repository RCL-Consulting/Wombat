# SsoMappings

SSO mappings (`/admin/sso/group-mappings`, `Pages/Admin/Sso/GroupMappings.razor`; its words in `SsoMappingWords.cs`, the commands `CreateSsoGroupMapping`, `DeleteSsoGroupMapping`, their refusals `SsoMappingRefusedException`): **which of an institution's sign-in groups give which Wombat role**, applied at each person's next sign-in. Flow 12 (T376, 2026-10-23, e53d4ae7; boards R3-O01 to O10; Q11, Q19, F8, S4, S5, S22, S23, M8; T264, T288, T326, T333) rebuilt her view. Until then she was shown the platform's settings key with no provider, a form that asked an institution and a speciality whatever the role, a red row Delete that asked nothing, and mappings from a removed provider mixed with the live ones.

## Who reads it

Administrator and Institutional admin. Her mappings are her institution's providers' only (T288). Someone who holds Trainee beside an admin role is shown the standing note `#sso-mappings-trainee-note` and nothing is read. **No mapping gives or takes Trainee or Pending trainee** (T333, `WombatRoles.AdmissionManaged`): the role select never offers them.

## The page

The header: "SSO mappings", "Which of your institution's sign-in groups give which Wombat role. A change applies at each person's next sign-in." (the Administrator's subtitle as built). In `.people-page`, the result region (hidden while empty), then:

- **No provider for her institution** (O01, Q11, T326): `section.detail-card.not-set-up`, its h2 "Institutional sign-in is not set up" with `key-round` in `muted-text`, then "Institutional sign-in is not set up for ‹institution›. A Wombat administrator sets it up; ask them if your hospital signs staff in through its own system." and "Until then, your people sign in with an email and a password, and there is nothing to change on this page." Standing content, no live region (S23); no form and no list. The Administrator's card names the `Sso:Providers` setting, as built (O02).
- **Add a mapping**: `form.form-container` named by its h2 (1.25rem, `.people-page .form-container > h2`); "* marks a field you must fill in. Speciality and Sub-speciality appear once a role that needs them is chosen."; on `.form-grid`s:
  - **Provider**: one is read out (`dl.form-group`, `dd.readonly-value`, "The one provider set up for ‹institution›."); several are a select.
  - **External group ID \***, **Group display name \***.
  - **Wombat role \***: a select starting "Choose a role", the staff roles by label (Institutional admin offered to the Administrator only, Q19), its help "Staff roles only. Trainee and Pending trainee are never mapped: a registrar becomes a trainee only when admitted."
  - **Institution**: read out, the provider's (T288).
  - **Speciality** and **Sub-speciality** only once the role uses them (S4): read out where the institution has adopted one ("The only ones ‹institution› has adopted."), a select otherwise; Coordinator and Committee member start at "Any speciality".
  - **Add mapping** (`plus`, primary).
- **Refusals** under their field, focused, no summary (S22): "Type the group's id, as the provider sends it.", "Type the group's display name.", "Choose a role.", the scope's two sentences, and a posted role the command refuses beside Wombat role (O06: "Only a Wombat administrator can map a group to Institutional admin.").
- **Added**: "**Mapping added.** People in ‹group› get Assessor in Paediatrics / Paediatrics at their next sign-in.", focused, the form empty.
- **Current mappings**: h2 with its count; `DataTable` `StackWide` captioned "At ‹institution›. Each applies at a person's next sign-in.": External group (the row header, `.person-cell`, the display name over its id in `span.group-id`, which breaks anywhere: an identity provider's id has no word to keep, build review P3), Provider, Wombat role and scope (`.person-cell`, the role over its scope), the Institution for the Administrator, and each row's **Delete…** (`trash`, `.btn-outline .btn-sm`, named "Delete the mapping of ‹group› to ‹role›", `aria-haspopup="dialog"`). None: "No group gives a role yet. Use Add a mapping to give one."
- **Delete…'s dialog** (ConfirmDialog, `DangerAction`, F8): "Delete this mapping?", "Delete the mapping of **‹group›** to **‹role›** (‹scope›)?", "From each person's next sign-in, belonging to ‹group› no longer gives ‹role›.", "The scope it gave stays."; **Keep the mapping** · **Delete the ‹group› mapping**. Deleted: "**Mapping deleted.** From each person's next sign-in, ‹group› no longer gives ‹role›.", focused.
- **Mappings left from a removed provider** (S5; `SsoGroupMappingDto.ProviderGone`): listed apart under their own h2, read only for her ("Read only: its provider is no longer set up." in the Actions cell), captioned that they give no new role, the roles they gave remain, and only a Wombat administrator can delete them; the delete command refuses her too. The Administrator deletes them.

## States

- **Loading**: "Loading the SSO mappings." over a skeleton card. **Failed**: "Could not load the SSO mappings. Nothing has changed. Try again, or come back in a few minutes." with Try again. A failed change: "Could not change the SSO mappings. Nothing has changed. …".

## Narrow widths

The tables stack at 900px and below, Delete… 44px from there; below 641px every control 44px and Add mapping the form's width. The replay's institution has no provider, so the replay reached only the not-set-up card (measured at 1280 and 390, T376 step 8); the form and the lists were checked by the states pass on a scratch instance with a provider configured.

## Contrast

No new colour token; the group's id and the scope `muted-text` 5.09:1.
