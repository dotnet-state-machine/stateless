# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
### Changed
### Deprecated
### Removed
### Fixed

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
