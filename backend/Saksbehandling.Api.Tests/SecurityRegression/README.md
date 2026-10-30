# Security regression tests — training fixtures

Every test in this folder is tagged `[Trait("Category", "SecurityRegressionTrainingFixture")]`
and currently **asserts the known, intentional vulnerable behavior** documented in
`docs/VULNERABILITY-MATRIX.md` (and, once published, `training/VULNERABILITY_MATRIX.md`).
That is deliberate — these are not bugs in the test suite.

**The workflow these tests are built around:**

1. A student finds a vulnerability (through the browser, the API, or source review).
2. They fix the underlying issue in the application code.
3. The matching test in this folder then **starts failing**, because it was asserting
   the insecure behavior.
4. That failure is the signal to update the test: flip its assertion to expect the
   now-secure behavior (403 instead of 200, a rejected payload instead of an accepted
   one, an empty result instead of leaked data, etc.), and move on to the next finding.

Run only these tests with:

```
dotnet test --filter Category=SecurityRegressionTrainingFixture
```

Run everything except these (the plain functional suite) with:

```
dotnet test --filter Category!=SecurityRegressionTrainingFixture
```

A handful of representative vulnerabilities are covered here, not all ~41 — this is a
sample demonstrating the pattern, not exhaustive coverage. Each test names the
vulnerability ID it documents in its own name and a one-line comment, for traceability
back to the matrix.
