# FSMP.Tests

Test suite for the FSMP solution — unit tests, integration tests, and test helpers.

## Responsibilities

**This project is responsible for:**
- Unit tests for all non-test projects
- Integration tests for cross-layer interactions
- Test helpers and utilities (builders, fakes, fixtures)
- Code coverage verification (minimum 80%)

**This project is NOT responsible for:**
- Production code of any kind

## How It Fits In

Testing Layer — references FsmpLibrary, FsmpDataAcsses, and other projects under test. No production project depends on this project.
