# Contributing to Stateless

If you're reading this page, thank you for considering making a contribution to Stateless! This project depends on the work of the community. As maintainers, we'll try our best to make bug fixes, review PRs, and respond to issues and feature requests.

## Getting Started

If you've found a bug, need a new feature or want to suggest a change, be sure to take a look through the [issues](https://github.com/dotnet-state-machine/stateless/issues?q=is%3Aissue) and [pull requests](https://github.com/dotnet-state-machine/stateless/pulls) in case it's been discussed before or is already in progress.

If you've found a security vulnerability, please report it using the [Security Advisories](https://github.com/dotnet-state-machine/stateless/security/advisories) page.

For anyone new to contributing to open source, there are some great guides to help you get started, such as [this one by freeCodeCamp.org](https://github.com/freeCodeCamp/how-to-contribute-to-open-source) and [this one from Open Source Guides](https://opensource.guide/how-to-contribute/).

## General Guidance

It's best to start by discussing a proposed change in an [issue](https://github.com/dotnet-state-machine/stateless/issues), be it a new issue you've created or an existing issue you're willing to help with. Let others know you're working on it.

Check that your [fork is synced with the upstream repo](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/working-with-forks/syncing-a-fork) before you start working on a change, then create a branch from it and commit your changes to that branch. When you're ready for the change to be reviewed, create a pull request to merge your change back to the upstream `dotnet-state-machine:main` branch.

Please make sure your change meets the following criteria before raising a pull request:

* Keep the change relevant to the specific issue being addressed; making changes that haven't been discussed could have unintended side effects.
* Add unit test coverage for your work; this not only helps to validate that your change works as described, it also acts as documentation and helps to defend against future regressions.
* Update the documentation! Help others benefit from your work by including guidance in the README.
* Be open and encouraging to constructive feedback; reviewers may ask for further changes or may want to discuss alternative approaches; the project will benefit from your patience and your willingness to engage with reviewers.

## Releasing (maintainers)

Package versions are calculated by [MinVer](https://github.com/adamralph/minver) from git tags, so there is no version number to edit in the project file. Builds of `main` are published to NuGet as pre-release versions (e.g. `5.21.1-alpha.0.3`).

Contributors should add a line describing their change to the `## Unreleased` section of [CHANGELOG.md](CHANGELOG.md). To publish a release:

1. In a pull request to `main`, rename `## Unreleased` in the CHANGELOG to `## X.Y.Z - YYYY.MM.DD` (following [Semantic Versioning](https://semver.org/)), and add a new empty `## Unreleased` section above it.
2. Once merged, update your local `main` and review any workflow changes since the previous release. The release runs the workflow files from the tagged commit, so check nothing unexpected has changed under `.github/`:
   ```
   git checkout main && git pull
   git log <previous tag>..HEAD -- .github/
   ```
3. Tag the merge commit and push the tag:
   ```
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```
4. The [Release workflow](.github/workflows/Release.yml) builds, tests and publishes the package to NuGet, then creates a GitHub Release using the CHANGELOG section as release notes. It fails before publishing anything if the tag isn't a valid `vX.Y.Z` or `vX.Y.Z-prerelease` version, if the tagged commit isn't on `main`, or if the CHANGELOG has no section for the tagged version.

Fixes are made on `main` and released from there; there are no release or maintenance branches, and the Release workflow only accepts tags on `main`. If a critical fix ever needs backporting to an earlier version, support for maintenance branches will be added then.

Releases are immutable, and NuGet packages can't be deleted. If a release is bad, don't delete it: fix the problem on `main` and release a new patch version. A bad package can also be [unlisted](https://learn.microsoft.com/nuget/nuget-org/policies/deleting-packages) on NuGet.

## Other Ways to Contribute

* Participate in open discussions, for instance by helping to answer questions or offering guidance to others.
* Improve the documentation, even something as small as fixing a typo or including a code snippet in the README; it all helps.
* Review a pull request; take a look through the [open pull requests](https://github.com/dotnet-state-machine/stateless/pulls) and offer constructive feedback.
* Boost the project; star the repository, mention it on social media, or link to it in your project's README.
