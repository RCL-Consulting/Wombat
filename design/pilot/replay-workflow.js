// Kept from T335 step G (2026-09-27): the acceptance replay as a Workflow script. To reuse it for a later flow:
// - install the Playwright library in a scratch folder (npm install playwright@1.63; Chrome is used through
//   channel "chrome", so no browser download), and point TMP and the import URL at that folder;
// - replace the database name, the snapshot names (scenario-<flow>-post-act<N>) and the replay's label;
// - start the app first: tools/scenario-replay.ps1 create/publish/start on a fresh wombat_scenario_<flow>.
export const meta = {
  name: 't335-step-g-replay',
  description: 'T335 step G: replay the whole scenario runbook on a fresh database against b347e11c, then capture flow 01 states',
  phases: [
    { title: 'Replay', detail: 'acts 1-6 then the appendix, in order, one agent each, on wombat_scenario_t335 at :5180' },
    { title: 'States', detail: 'flow 01 states from the new snapshots, three sections in parallel on scratch ports' },
  ],
}

const TMP = 'C:\\Users\\Renier\\.claude\\jobs\\2a61f9ad\\tmp'
const RB = 'execution/knowledge/scenario-paediatrics'
const ACTS = [
  { key: '1', file: 'act-1-setup.md', dir: 'act-1' },
  { key: '2', file: 'act-2-onboarding.md', dir: 'act-2' },
  { key: '3', file: 'act-3-operations.md', dir: 'act-3' },
  { key: '4', file: 'act-4-annual-review.md', dir: 'act-4' },
  { key: '5', file: 'act-5-graduation.md', dir: 'act-5' },
  { key: '6', file: 'act-6-catalogue.md', dir: 'act-6' },
  { key: 'A', file: 'appendix-cross-cutting.md', dir: 'act-A' },
]

const ACT_SCHEMA = {
  type: 'object',
  properties: {
    act: { type: 'string' },
    steps_total: { type: 'number' },
    steps_passed: { type: 'number' },
    gaps: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          step: { type: 'string' },
          kind: { type: 'string', enum: ['regression-t335', 'known-task', 'runbook-wrong', 'new-defect', 'not-played'] },
          task: { type: 'string' },
          summary: { type: 'string' },
        },
        required: ['step', 'kind', 'summary'],
      },
    },
    dump: { type: 'string' },
    blocking: { type: 'boolean' },
    notes: { type: 'string' },
  },
  required: ['act', 'steps_total', 'steps_passed', 'gaps', 'blocking', 'notes'],
}

const STATES_SCHEMA = {
  type: 'object',
  properties: {
    section: { type: 'string' },
    captured: { type: 'number' },
    mismatches: { type: 'array', items: { type: 'object', properties: { state: { type: 'string' }, summary: { type: 'string' } }, required: ['state', 'summary'] } },
    not_captured: { type: 'array', items: { type: 'object', properties: { state: { type: 'string' }, why: { type: 'string' } }, required: ['state', 'why'] } },
    notes: { type: 'string' },
  },
  required: ['section', 'captured', 'mismatches', 'not_captured', 'notes'],
}

const COMMON = `
You are playing part of T335 step G: the acceptance replay of Wombat's whole scenario runbook, on a fresh database,
against the flow 01 restructure (the shell), which landed on master as b347e11c. The repo is C:\\Users\\Renier\\Wombat
(branch master; do not switch branches, do not commit, do not push).

The replay app is ALREADY RUNNING: http://localhost:5180, database wombat_scenario_t335, published from b347e11c,
started by the integrator with tools\\scenario-replay.ps1 (Development, mail written to the log
C:\\Users\\Renier\\Wombat\\.scenario-app\\wombat_scenario_t335.log as "Stub email" lines, emailed links pointing at :5180).
The acts before yours have been played on it, so its state is theirs. If the app has died (no answer on :5180), restart
it with: powershell -File tools\\scenario-replay.ps1 start wombat_scenario_t335 5180 ; never publish again, never
create or drop a database, and never touch port 5080 (the dev app) or its database.

Read first: ${RB}/README.md (How to play; The step format), then your act file in full.

Browser: the Playwright library is installed at ${TMP}\\pw\\node_modules (v1.63). Write your scripts under
${TMP}\\g\\<your act dir>\\ and import it by absolute file URL, e.g.
  import { chromium } from 'file:///C:/Users/Renier/.claude/jobs/2a61f9ad/tmp/pw/node_modules/playwright/index.mjs';
Launch with chromium.launch({ channel: 'chrome', headless: true }). ${TMP}\\pw\\lib.mjs shows a working launch, login
and waitBlazor (it points at :5080; use :5180). Signed-in pages are Blazor Interactive Server: wait for the circuit
before acting. Default viewport 1280x800; a step that says 390 px uses 390x844.

Accounts and passwords:
- Dev accounts (devadmin@wombat.local and the others): their dev-only passwords are constants in
  src/Wombat.Infrastructure/Identity/DevUserSeeder.cs.
- Cast accounts registered during the replay: choose a strong password at replay time and keep it ONLY in
  ${TMP}\\g\\replay-pw.env as KEY=value lines (WB_PW_<SURNAME>=...), which later acts read into their scripts'
  environment. Never print a password to the console or into your reply, never write one into the runbook or any
  tracked file, and never open pwd_DO_NOT_COMMIT.txt or recovery\\.
- Registration and MSF links come from the log's Stub email lines; never write a token into the runbook.

The clock: D is the replay day (2026-09-27); J is the latest 15 January on or before D. Dates in steps are relative.

SQL checks: write a .sql file under your tmp dir and run
  powershell -File tools\\scenario-replay.ps1 sql wombat_scenario_t335 <file.sql>

For EVERY step in your act, in order:
1. Play its Role, Route and Do as written.
2. Check every clause of its Expect against what the app shows (and the log or SQL where the Expect says so).
3. Replace the step's Actual line (and its continuation lines) with ONE new Actual:
     Actual (2026-09-27, T335 replay, wombat_scenario_t335): <what you saw, concise; quote on-screen words where the
       Expect does>
   and replace its Gap line with "Gap: none" or the finding. If the Expect fails because of a still-open task the old
   Gap line cites, keep citing that task (for example "Gap: F-2.34a, T317 (still: ...)"). For a new finding write
   "Gap: new: <one sentence>". Keep the file's style: lines under 120 characters, continuations indented two spaces.
4. Do NOT edit Role, Route, Do, Expect or Note lines. If a step cannot be played as written, or its Expect is wrong
   about what the design decided, record that in its Gap ("Gap: runbook: ...") and play on as far as you sensibly can.
5. Capture the step's outcome screenshot(s) to design\\baseline\\<your act dir>\\<step>-<n>-<slug>.png, replacing the
   old capture(s) for that step (the folder is gitignored and local only). Say in your notes which captures show a
   registration link.
Edit no file but your act file (and your tmp dir and the baseline images).

At the end of your act: check its outcome state (the act file's closing SQL, if any), then snapshot it:
  powershell -File tools\\scenario-replay.ps1 dump wombat_scenario_t335 scenario-t335-post-act<your act key>
and leave the app running for the next act.

Classify each Gap for the integrator:
- regression-t335: the flow 01 build (shell, nav, Home, failure pages, titles, tokens, reconnect, acting role) made
  it wrong.
- known-task: an open task the Gap already cites still fails.
- runbook-wrong: the code is right by the design and the runbook line is wrong.
- new-defect: a defect not from T335 and not already filed.
- not-played: you could not play it (say why).
Set blocking=true only if the app or the database is broken so that the next act cannot run.
`

function actPrompt(act) {
  return `${COMMON}
YOUR ACT: ${act.key}. Your file: ${RB}/${act.file}. Your act dir: ${act.dir} (screenshots in design\\baseline\\${act.dir},
scripts in ${TMP}\\g\\${act.dir}). Your snapshot name: scenario-t335-post-act${act.key}.
${act.key === 'A' ? 'The appendix runs last; its steps state their own preconditions and some change shared state.' : ''}
Return the structured result: steps_total, steps_passed (Gap: none), every Gap with its kind, the dump name, and notes
(anything the next act must know, which captures show a registration link, anything you could not check).`
}

function statesPrompt(section, port) {
  return `
You capture the flow 01 states of Wombat's replay baseline for T335 step G (the shell restructure, b347e11c on master,
repo C:\\Users\\Renier\\Wombat; do not commit, do not switch branches).

Your section of ${RB}/states.md is: "${section}". Read states.md's intro and "How to capture", then every row of your
section. For each row: reach the state as its "How to reach it" says, check what it says the screen shows, and capture
it to the file its Screenshot column names under design\\baseline\\ (gitignored; replace the old file).

Where a row needs a scratch database ("Scratch (post-actN)" or any row that changes data or holds a lock), NEVER use
wombat_scenario_t335 or port 5180 (the acceptance replay's) or 5080 (dev). Instead:
  powershell -File tools\\scenario-replay.ps1 restore scenario-t335-post-act<N> wombat_scenario_t335_states_${port}
  powershell -File tools\\scenario-replay.ps1 start wombat_scenario_t335_states_${port} ${port}
(the snapshots recovery\\scenario-t335-post-act<N>.dump were written by the replay: 1..6 and A), play from there, and
stop your app at the end with: powershell -File tools\\scenario-replay.ps1 stop ${port}. You may restore a different
snapshot into a new name (wombat_scenario_t335_states_${port}_<n>) when a row needs another act's state; the tool never
drops a database, so just make new names. Rows that only read may use http://localhost:5180 read-only (sign in, look,
change nothing).

Browser: the Playwright library at ${TMP}\\pw\\node_modules (v1.63); write scripts under ${TMP}\\g\\states-${port}\\ and
import it by absolute file URL (file:///C:/Users/Renier/.claude/jobs/2a61f9ad/tmp/pw/node_modules/playwright/index.mjs);
chromium.launch({ channel: 'chrome', headless: true }); ${TMP}\\pw\\lib.mjs shows login and waitBlazor. Viewports as the
row says (1280x800 default, 390x844 for narrow rows). Reduced motion or forced colours: use page.emulateMedia.
Passwords: dev accounts' constants are in src/Wombat.Infrastructure/Identity/DevUserSeeder.cs; the cast's are in
${TMP}\\g\\replay-pw.env (KEY=value; load into your script's environment). Never print a password, never open
pwd_DO_NOT_COMMIT.txt or recovery\\ files directly, never write a password or a token anywhere tracked.

The error page after a real failure is served outside Development only; if you cannot run the app outside Development
without reading a secret, record that state as not captured with that reason. Reconnect states: suspend or stop your
own scratch app as states.md's recipes say (never 5180 or 5080).

Edit no tracked file. Return: captured count, each mismatch between a row's description and the screen, each state not
captured with why, and notes.`
}

phase('Replay')
const results = []
for (const act of ACTS) {
  log(`Act ${act.key}: replaying ${act.file}`)
  const r = await agent(actPrompt(act), { label: `replay act ${act.key}`, phase: 'Replay', schema: ACT_SCHEMA })
  if (!r) { log(`Act ${act.key}: no result; stopping the replay`); results.push({ act: act.key, missing: true }); break }
  results.push(r)
  log(`Act ${act.key}: ${r.steps_passed}/${r.steps_total} steps with no gap; ${r.gaps.length} gaps`)
  if (r.blocking) { log(`Act ${act.key} reported blocking; stopping before the next act`); break }
}

let states = []
const replayedAll = results.length === ACTS.length && results.every(r => r && !r.missing && !r.blocking)
if (replayedAll) {
  phase('States')
  const SECTIONS = [
    { section: 'Shell and framework', port: 5184 },
    { section: 'Home and the role dashboards', port: 5185 },
    { section: 'System pages', port: 5186 },
  ]
  states = await parallel(SECTIONS.map(s => () =>
    agent(statesPrompt(s.section, s.port), { label: `states: ${s.section}`, phase: 'States', schema: STATES_SCHEMA })))
} else {
  log('The replay did not reach the end; the states phase is skipped')
}

return { acts: results, states: states.filter(Boolean) }
