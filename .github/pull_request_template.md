# Description

*Please provide a detailed description. Be as descriptive as possible - include information about what is being changed,
why it's being changed, and any links to relevant issues. If this is closing an existing issue use one of the [issue linking keywords](https://docs.github.com/issues/tracking-your-work-with-issues/using-issues/linking-a-pull-request-to-an-issue#linking-a-pull-request-to-an-issue-using-a-keyword) to link the issue to this PR and have it automatically close when completed.*

In addition, go through the checklist below and check each item as you validate it is either handled or not applicable to this change.

# Code Changes

- [ ] [Unit tests](/test/) are added, if possible
- [ ] Existing [tests are passing](https://github.com/microsoft/DacFx/actions/workflows/pr-validation.yml)
- [ ] New or updated code follows the guidelines [here](/CONTRIBUTING.md)
- [ ] Ensure .dacpac from changes to Microsoft.Build.Sql build process MUST be backwards compatible with older versions of SqlPackage
- [ ] Use proper logging for MSBuild tasks
