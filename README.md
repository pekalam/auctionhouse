SQL Server:&nbsp;&nbsp;&nbsp;[![Build Status](https://dev.azure.com/marekbf3/auctionhouse/_apis/build/status/pekalam.auctionhouse?branchName=sqlserver)](https://dev.azure.com/marekbf3/auctionhouse/_build/latest?definitionId=1&branchName=sqlserver)

Eventstore:&nbsp;&nbsp;&nbsp;[![Build Status](https://dev.azure.com/marekbf3/auctionhouse/_apis/build/status/pekalam.auctionhouse?branchName=eventstore)](https://dev.azure.com/marekbf3/auctionhouse/_build/latest?definitionId=1&branchName=eventstore)


**Frontend**

Angular8 + material components

**Backend**

**CQRS and Event sourcing**

I've decided to experiment with following architecture:

![ScreenShot](https://raw.githubusercontent.com/pekalam/auctionhouse/master/docs/img/Arch.png)

This system makes use of 2 databases - command database and query database. Command part of system changes state of the system and stores generated events in its database.
Command and query database must be consistent so generated events are sent with the use of message queue and handled by the query part of system to populate MongoDB database data. 

Event sourcing command database acts as the single source of truth - the query database can be dropped and recreated by sending events over the message queue. It is also a great source of logs and analytical data. 

Application of CQRS also provided an ability to use message queue for long running tasks and decrease of throttling. It also provided an ability to ged rid of large service classes.

In case of this domain, this architecture introduced several drawbacks asociated with buisness transactions and concurrency. In order to resolve them I've implemented:
- offline lock pattern (aggregate root versioning)
- command rollbacks (error queue)

which resolved in a little bit complicated system.

**Database**

Commands:

Initialy I started with EventStore but its lack of transactions introduced several issues. I've decided to use SQL Server and implement my own event sourcing database. 

Queries:

Query part of system is using MongoDB. I've used sharding and replication.

**External systems**

- RabbitMQ 
- quartzWebTaskService - my own project (https://github.com/pekalam/quartzWebTaskService), web api for Quartz.NET. Used to end an auction at desired time.   

**Domain**

Main logic of application is represented by Domain Model.

Main parts of domain model:
- Auction: 
    - can be created by logged in user
    - users can subbmit a bid or buy it depending of type of an auction
- User:
    - can create, delete an auction and bids
    - has an account with credits

**Testing**

Backend is fully tested. Test.* projects contain domain, unit, integration and functional tests which can be run locally or in containers. Tests are run on azure pipelines.

**Containerization**

Backend application and tests are containerized. I've used built-in Docker orchestration.  

**Package level overview**

![ScreenShot](https://raw.githubusercontent.com/pekalam/auctionhouse/master/docs/img/PackageOverview.png)

**Description of each package:**

- **Core.Common**:
    - domain model
    - interfaces, base classes, services for Core.Command/Query
    - interfaces (ports) for Infrastructure and Web packages
- **Core.Command**:
    - commands (use cases)
- **Core.Query**:
    - queries (use cases)
- **Infrastructure**:
    - adapters for Core.Common ports
    - bootstraps application (DI, initialization of services)
- **Web**:
    - web api for frontend
    - adapters for Core.Common ports
