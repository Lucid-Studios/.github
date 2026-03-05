# Phase 2 Audit Report

- Pass: True
- Flow Score: 45
- SCAR Coverage: 1

## Determinism
- Pass: True

## Contract Checks
- determinism_pass: True
- rootindex_unchanged: True
- reserved_hard_ban: True
- reserved_literals_only: True
- uniqueness_scope_expanded: True
- fractional_metadata_only: True
- telemetry_layer0_scar: True
- telemetry_layer_placeholders: True
- glue_isolation: True
- coverage_report_present: True
- coverage_target: True

## Coverage Breakdown
- pos=0 text='The' node= reason=unmapped_stopword eligible=False
- pos=1 text='firefighter' node=e1 reason=mapped_explicit eligible=True
- pos=2 text='entered' node=ev1 reason=mapped_explicit eligible=True
- pos=3 text='the' node= reason=unmapped_stopword eligible=False
- pos=4 text='burning' node=st1 reason=mapped_explicit eligible=True
- pos=5 text='building' node=e2 reason=mapped_explicit eligible=True
- pos=6 text='.' node= reason=unmapped_punctuation eligible=False
