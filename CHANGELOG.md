# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## Unreleased
### Added
### Changed
### Deprecated
### Removed
### Fixed

## [4.4.0] - 2020.02.07
### Changed
- PR #348 Added trigger parameters to OnTransitioned event
### Fixed
- #342 StateMutator is called multiple times with the same state 
- #345 Wrong behavior with FiringMode.Immediate in version 4.3.0
- #339 OnEntry behavour different: Unit test broken when updated to 4.3.0 from 4.2.1
- #292 Nested InitialTransitions calls superstate onEntry twice

## [4.3.0] - 2019.11.11
### Changed
- Netstandard2.0 support

## [4.2.0] - 2018.06.01
### Added
 - PR #254 Add initial transitions
### Changed
- Simplified examples a bit and added comments 
### Fixed
- #261 Reentrant trigger in substate causes exit action to be executed twice
- #263 NullReferenceException bug introduced in commit c13e181b

## [4.1.0] - 2018-05-18
### Added
 - #246 Queuing should not be the default behavior
### Fixed
 - #191 Shouldn't Reentry fire OnEntry from substate?
 - #228 guard function called twice?
 - #214 Transition object holds parent state as source state instead of child
 - #249 Latest prerelease v4.0.1-dev-00294 bug (Internal transition handler not executing)

## [4.0.0] - 2017-09-30
### Added
 - #169 HTML format DOT graph output
 - #132 Conditional InternalTransition
 - #130 Support InternalTransition for all TriggerWithParams  
### Changed
 - #178 Replace calls to Enforce.ArgumentNotNull with in-place null checks, and other changes suggested by CodeCracker
 - #168 Modified the output of OnUnmute to display the correct message
 - #164 Reducing confusion, and fixing invalid cast in Reflection.  
 - #158 Create new ActionInfo class, start insinuating it
 - #156 TriggerInfo.Value -> UnderlyingTrigger  
 - #149 Moved StateType and TriggerType into StateMachineInfo, removed TriggerInfo
 - #147 Extracting common abstract base class for transition info
 - #142 Further reflection API refinement
 - #141 Update to RTM dotnet tooling/VS2017
 - #137 Replace magic string usages with nameof()
 - #125 Adding check for cyclic configuration
 - #123 Update for the current .NET Core version  
 

### Deprecated
### Removed
### Fixed
 - #175 Fix Multiple internal transition actions executed if defined in state hierarchy, and Renamed DynamicTransitionInfo.Destination to DestinationDescription  
 - #144 Fix unbounded recursion/fully initialize sub/super-states
 - #140 Adding recursive trigger processing to InternalFireAsync  

## [3.1.0] - 2016-12-15
### Added
### Changed
### Deprecated
### Removed
### Fixed

## [3.0.1] - 2016-11-23
### Added
### Changed
### Deprecated
### Removed
### Fixed

## [3.0.0] - 2016-11-03
### Added
### Changed
### Deprecated
### Removed
### Fixed

 4.2.0]: https://github.com/dotnet-state-machine/stateless/commit/8933fe58a3d2ab63bdf47f523df0b9639cd65c97
[4.1.0]: https://github.com/dotnet-state-machine/stateless/compare/bb742e8d40ceaacb219695875dfe38670ac77e28...daef9cb2897e18f25e85dd27fb80e549369bdfac
[4.0.0]: https://github.com/dotnet-state-machine/stateless/compare/23624d88e684d9984e5b5fdbc3d4aba601bdd1a4...bb742e8d40ceaacb219695875dfe38670ac77e28
[3.1.0]: https://github.com/dotnet-state-machine/stateless/compare/6aa544c6a5e22b93fbe206513d79e15a3e2ef172...23624d88e684d9984e5b5fdbc3d4aba601bdd1a4
[3.0.1]: https://github.com/dotnet-state-machine/stateless/compare/6c44d2ae69f67606b5d979b2f0e353adccb1913c...6aa544c6a5e22b93fbe206513d79e15a3e2ef172
[3.0.0]: https://github.com/dotnet-state-machine/stateless/compare/4d4cc84bad583eaf8f983edc351179cef40bd093...6c44d2ae69f67606b5d979b2f0e353adccb1913c
