# Data Classification

Lucid Studios uses a simple repository-facing classification model:

- `Public`: safe for public repository visibility
- `Internal`: non-public operational or design information
- `Restricted`: sensitive information that must not be committed publicly
- `Cryptic`: governed references or payload-adjacent information that should remain pointer-only in public-safe contexts

Repositories may refine this model, but should not collapse it below this baseline without explicit approval.
