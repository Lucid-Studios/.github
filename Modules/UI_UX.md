```markdown
# OAN Mortalis Workbench
## UI / UX Specification (MVP v0.1)

Document Status: Draft  
Target Engine: OAN Mortalis Headless Spine (.NET 8)  
Scope: Deterministic Workbench around Engrammitization v0.1  
Non-Goal: No architectural changes to SLI, SAT, or identity pipeline  

---

# 0. Design Principles

## 0.1 Engine Authority

- The Engine is the only authority.
- The UI never mutates identity state directly.
- The UI never writes to ledger files.
- The UI never computes identity hashes.
- The UI never infers permissions.

All writes occur via Engine API calls.

## 0.2 Determinism

- UI timestamps are display-only.
- Ordering is defined by telemetry `seq`.
- UI must not reorder events.
- UI must not batch or debounce identity-affecting calls.

## 0.3 Identity Invariant

Identity advances only when:

```

IntentStatus == Committed

```

UI transitions must never cause:
- HodgeTheaterSeeded
- EngrammitizedEventAppended
- OpalTipAdvanced

---

# 1. Application State Machine

```

Splash
↓
Login
↓
WorkspaceSelected
↓
ConnectorConfigured
↓
CMESelected
↓
Booting
↓
Running
↙        ↘
Parked   Terminated

```

Each transition must correspond to explicit Engine API calls.

---

# 2. Screen Specifications

---

# 2.1 Splash Screen

## Purpose
Attach or start Engine process.

## UI Elements

- App logo
- Engine status indicator
- Version display
- Build hash
- RootAtlas hash (if remembered)
- Continue button (enabled when engine ready)

## Engine Calls

GET /api/engine/status

## Telemetry

None required.

## Acceptance

- No identity events created.
- No workspace interaction.

---

# 2.2 Login Screen

## Purpose
Authorize operator role.

## Roles

- Operator (HITL enabled)
- Viewer (read-only)

## UI Elements

- Role selector
- Passphrase/PIN
- Login button

## Engine Calls

POST /api/auth/login

Returns:
- sessionToken
- role

## Telemetry

AuthSessionOpened

## Acceptance

- Viewer cannot approve SAT elevation.
- Login does not change SAT mode.
- Login does not create identity events.

---

# 2.3 Workspace Selection

## Purpose
Load workspace folder.

## UI Elements

- Folder picker
- Recent workspace list
- Validation panel
- Open button

## Validation Display

- Workspace schema version
- RootAtlas hash
- Policy version
- Available CMEs
- Last run summary

## Engine Calls

POST /api/workspace/open  
GET /api/workspace/summary

## Telemetry

WorkspaceOpened  
RootAtlasLoaded

## Acceptance

- Missing RootAtlas blocks open.
- Opening workspace does not seed theater.

---

# 2.4 Model Connector Selection

## Purpose
Configure LLM connector.

## Supported Types (MVP)

- OpenAI
- LocalHTTP
- vLLM
- Ollama
- Custom DLL

## UI Elements

- Connector type dropdown
- Model ID field
- Endpoint field
- API key field (secure storage)
- Test Connection button
- Save button
- Continue button

## Engine Calls

POST /api/connectors/configure  
POST /api/connectors/test

## Telemetry

ConnectorConfigured  
ConnectorTested

## Acceptance

- Test must not produce commits.
- Connector errors must not alter identity state.

---

# 2.5 CME Selection

## Purpose
Select identity envelope.

## UI Elements

- CME list
- Details panel:
  - Last OpalTip (if persisted)
  - Last SAT mode
  - Mount profile
- Boot button
- Boot Read-Only button
- Replay Run button

## Engine Calls

GET /api/cmes  
POST /api/session/create

## Telemetry

SessionCreated

## Acceptance

- Session creation must not seed theater.
- Read-only blocks CommitIntent.

---

# 2.6 Booting Screen

## Purpose
Deterministic preflight.

## Steps Displayed

1. RootAtlas load
2. Mount plan application
3. SAT mode initialization
4. Driver readiness

## UI Elements

- Progress list
- Live telemetry tail
- Abort button

## Engine Calls

POST /api/session/boot  
GET /api/session/status

## Telemetry

MountPlanApplied  
SatModeSet  
DriverReady

## Acceptance

- Abort must not create commits.
- Boot must not seed theater.

---

# 2.7 Main Console

Tabs:

- Chat
- Tools
- Files
- Telemetry
- Admin

---

# 2.7.1 Chat Tab

## Purpose
User-driven ingestion.

## UI Elements

- Transcript window
- Input field
- Send button
- Attempt counter display
- Last intent result panel

## Engine Calls

POST /api/ingest

## Telemetry Displayed

Ingested  
DriverAttempt  
SliResolved  
IntentCommitted  
IntentRefused  
EngrammitizedEventAppended  

## Acceptance

- UI does not choose handles.
- Retries limited to 3.
- Only one identity advancement per committed intent.

---

# 2.7.2 Tools Tab

## Purpose
Explicit handle invocation.

## UI Elements

- Searchable handle list
- Required SAT modes display
- Dry-run resolve button
- Invoke button

## Engine Calls

POST /api/sli/resolve  
POST /api/intents/submit

## Telemetry

SliResolved  
IntentCommitted  
IntentRefused  

## Acceptance

- Dry-run must not commit.
- Invoke always goes through SLI.

---

# 2.7.3 Files Tab

## Purpose
Workspace file management.

## UI Elements

- File browser
- Import button
- Attach-to-chat selector

## Engine Calls

POST /api/files/import  
GET /api/files/list

## Telemetry

FileImported

## Acceptance

- File import does not alter identity.
- Attachments are explicit references only.

---

# 2.7.4 Telemetry Tab

## Purpose
Live NDJSON viewer.

## UI Elements

- Live stream window
- Filter chips
- Export run bundle button

## Engine Calls

GET /api/telemetry/stream  
POST /api/runs/export

## Acceptance

- Ordering defined by seq.
- Export includes:
  - telemetry.ndjson
  - seed events
  - tip chain
  - root atlas hash

---

# 2.7.5 Admin Tab

## Purpose
Governance + session control.

## UI Elements

- SAT elevation queue
- Approve button
- Deny button
- Park (TSR) button
- Terminate button

## Engine Calls

GET /api/sat/requests  
POST /api/sat/decide  
POST /api/session/park  
POST /api/session/terminate  

## Telemetry

SatElevationRequested  
SatElevationOutcome  
SessionParked  
SessionTerminated  

## Acceptance

- Only Operator role can approve.
- Park must not create commits.
- Terminate must release secrets.

---

# 3. Logout Semantics

## 3.1 TSR (Park)

- Stop driver loop.
- Close connector.
- Freeze mounts.
- Emit SessionParked.
- No identity writes.

## 3.2 Full Terminate

- Stop engine.
- Clear session.
- No continuity implied.
- Restart requires new session.

---

# 4. Workspace Layout (MVP)

```

/workspace
/config
/cmes
/runs
/telemetry
/exports

```

Engine writes all files.

UI reads via API only.

---

# 5. Accessibility

- Keyboard-first navigation.
- Clear state indicators (SAT mode visible at all times).
- Commit availability visibly gated.
- Viewer mode visually distinct.

---

# 6. Security Notes

- API keys stored via OS secure store.
- Session tokens never logged.
- Telemetry export excludes secrets.
- Operator decisions logged with operatorIdHash.

---

# 7. Non-Goals (MVP)

UI must not:

- Modify RootAtlas.
- Modify policyVersion.
- Auto-generate handles.
- Promote engrams.
- Perform learning.
- Mask cryptic domains.
- Inject timestamps into identity.

---

# 8. Acceptance Checklist

- No identity events on boot.
- No identity events on login.
- No identity events on park.
- No identity events on terminate.
- Exactly one EngrammitizedEvent per committed intent.
- Deterministic retry visibility.
- Append-only OpalTip enforcement.
- Viewer role blocks governance actions.

---

# 9. Future Extensions (Post-MVP)

- Tip visualization graph.
- Cross-run diff viewer.
- Persistent OpalTip storage.
- Replay engine mode.
- Fork detection UI.

---

END OF FILE
```
