# 3. Decouple Read Model Notifications

Date: 2022-11-18

## Status

Accepted

## Context

Read model notifications are going to be removed from project, they are making the code harder to read and will not be used after redesign of flows.

## Decision

Code related to ReadModelNotifications will be removed from modules and moved to 'Extensions' folder. Extensions will use an ability to decorate services and use command handlers 'hooks' e.g. 'OnExecute'. Implementation of one-way communication with extensions will provide an ability to use their functionalities based on current state of the application. Extensions should be easily removed from the application by removing code that is using them.

## Consequences

Risks related to change of this functionality should be reduced by improved integration tests.
