# Lucid Studios `.github`

This is the Lucid Studios organization `.github` repository.

It serves four roles:

- organization profile content through [`profile/README.md`](profile/README.md)
- default community health files for repositories that do not define their own overrides
- shared governance and policy documentation for engineering, AI, data, and contribution standards
- reusable GitHub workflows for repository health and standards checks

## How GitHub uses this repository

- `profile/README.md` is rendered on the Lucid Studios organization profile
- root-level and supported community-health files act as organization defaults for repositories that do not provide their own versions
- reusable workflows under [`.github/workflows`](.github/workflows) can be called from Lucid Studios repositories

## Repository Layout

- [`profile/README.md`](profile/README.md): organization-facing profile content
- [`CONTRIBUTING.md`](CONTRIBUTING.md): default contributor guidance
- [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md): organization collaboration rules
- [`SECURITY.md`](SECURITY.md): default security reporting guidance
- [`SUPPORT.md`](SUPPORT.md): support routing
- [`pull_request_template.md`](pull_request_template.md): default PR checklist
- [`.github/ISSUE_TEMPLATE`](.github/ISSUE_TEMPLATE): default issue templates
- [`docs`](docs): canonical Lucid Studios governance and policy documents
- [`.github/workflows`](.github/workflows): reusable and org-level workflows

## Override Model

Product repositories may override these defaults when they need stricter or domain-specific rules. Overrides should narrow or specialize the Lucid Studios baseline, not weaken it without maintainer approval.

## Scope

This repository is not the place for product implementation issues, roadmap work, or project-specific support requests unless the request is about shared org governance, templates, or workflows.
