[DRAFT]
# 2. Implemenent common style and architecture of unit tests in modules

Date: 2022-04-23

## Status

Accepted

## Context

Unit tests of aggregates that emit domain events are hard to read, understand and maintain. Tests are too big and some of them are not exaustive. Names are prone to SUT renaming and implementation change which may happen
often in this project. 

## Decision

The change that we're proposing or have agreed to implement.
Following decisions have been taken:
1. Apply common naming style
    * test names should be behaviour-oriented and not tied to one common schema because it's less prone to public api changes
2. Decide on definition and use concepts like
    * mocks
    * stubs
    * test doubles
    * fakes
3. Create test object factories inside project common for tests of all module layers and types
    * factories should begin with ``Given`` prefix and contain ``Build`` method - factories are addressed in 4th point
4. Apply common test architecture
    * test body should be split into Arrange Act Assert sections by newlines / comments or into Given When Then sections
    * in arrange section factories named with ``Given`` prefix should be used to provide better readability that adheres to splitiing into AAA / GWT convetion
    * bigger arrange section should be decomposed into private test methods following the same ``Given``-like naming convetion as factories
    * as FluentAssertions will be used in every test project, then assertions should be written in a way that when reading test from top to down, test should try to look like a specification containing ``Given`` words on top and ``Should`` on down and tested actions in between
    * skip private keyword and private field naming convetions for test classes - it's unnecessary noise in code for tests and class field can be distinguished by variable color in IDE
    * when testing if sut throws then it should be mandatory to test exact exception message

## Consequences

What becomes easier or more difficult to do and any risks introduced by the change that will need to be mitigated.
