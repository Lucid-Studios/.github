1) sli.resolved_address.v0.1.0
Resolved Address Normal Form (Layer 1, Deterministic)
Purpose

Defines the canonical normal-form address that every handle resolves to:

ResolvedAddress = Channel × Partition × Mirror 

SLI CONSTITUTION v0_1

Schema ID

sli.resolved_address.v0.1.0

Record
public record SliResolvedAddress_v0_1_0(
    string Schema,            // "sli.resolved_address.v0.1.0"
    string Channel,           // "Public" | "Private"
    string Partition,         // "GEL" | "GOA" | "OAN"
    string Mirror,            // "Standard" | "Cryptic"
    string AddressText,       // $"{Channel}/{Partition}/{Mirror}" (exact)
    string AddressHash        // sha256(AddressText UTF-8) lowercase hex
);
Freeze Rules

AddressText is the canonical string form.

No enum ToString() allowed; values must be exact case.

AddressHash must be computed from AddressText only.

Tests

RA-Addr-1: Same fields → same AddressText and AddressHash.

RA-Addr-2: Invalid enum value must be rejected (no “Any” here; “Any” belongs in constraints, not resolved addresses).