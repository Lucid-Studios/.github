# Contributing

This file defines the Lucid Studios default contribution baseline for repositories that do not provide a stricter local override.

## Baseline Expectations

- keep changes scoped and reviewable
- document behavior, risk, and dependency changes clearly
- do not commit secrets, private keys, or restricted data
- disclose material AI assistance in pull requests
- use DCO-style sign-off by default with `git commit -s`
- expect maintainers to request extra ownership clarification or a CLA when IP risk is unusually high

## Before Opening a Pull Request

- read the target repository `README.md`
- read local repository-specific contribution or security guidance if it exists
- review shared Lucid Studios standards in [`docs`](docs)
- ensure tests and validation relevant to your change have been run

## AI and Data Expectations

- distinguish direct observation, human inference, and generated content
- preserve provenance and attribution
- keep restricted material out of public repositories
- treat public-safe boundaries and disclosure rules as engineering requirements

## Override Rule

If a product repository defines its own `CONTRIBUTING.md`, that repository-specific guidance takes precedence for local workflows so long as it does not weaken the Lucid Studios baseline without maintainer approval.
