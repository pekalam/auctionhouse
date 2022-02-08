(2022 update)\
# !!Work in progress - some functionalities are not working

```
Demo mode accounts:
username: test1 password: pass
username: test2 password: pass
username: test3 password: pass
```

# Introduction

Web application that provides functionality such as buying items, selling items and creating auctions. It's kind of more complex POC. It's using **CQRS** and **event sourcing** patterns as well as  ports and adapters architecture. 
From the conceptual point of view, application is split into **modules** and into query and command operations that are reading and writing to separate databases. Modules are lousely coupled and communicate with each other by sending events. Coordination across modules is implemented with use of **saga pattern**. Query database is being synchronized with command database with use of event bus implemented by RabbitMQ, so system is **eventually consistent**.

## State of the project

Some of use-cases are still not working and they require to design some parts of a domain model and rewrite / test some parts of the infrastructure. Below you can read a list of planned functionalities

## Functionalities
### Account
* sign-in, sign-up
* ~~reset password~~
* ~~oauth~~
### Auctions
* adding new auction
* buy
* bidding
* pay with user credits (fake currency)
* ~~manage user credits~~
* ~~payment gateway integration~~
* searching
* listing and sorting
### Other
* demo mode
* ~~password hashing~~ (for development needs)

## Architectural and technical concerns

* Modular architecture, intended to be deployed as microservices, currently it's deployed as monolith (Command WEB API project)
* Saga pattern implementation
    * currently using custom fork of Chronice: https://github.com/pekalam/Chronicle and SQL Server integration https://github.com/pekalam/Chronicle.Integrations.SQLServer
* Event sourcing
    * Relational db storage
    * ~~Aggregate snapshots~~
* Event outbox implementation
* DDD tactical patterns (Aggregates, Value Object, Entity)
* Distributed tracing (Jeager)
* Unit, integration and functional tests
* Docker swarm deployment
* MongoDB / SQL server backups automated by scripts
* Envoy as edge proxy
* ~~CI~~

## C4 Container diagram

![ScreenShot](https://raw.githubusercontent.com/pekalam/auctionhouse/rewrite/docs/img/Auctionhouse_C4_2.png)


## How to start

### Local deployment

1. Start docker and run powershell script:
```
backend/src/Start-ComposeLocal.ps1
```
2. Open https://localhost:10000

### Development

1. Open solution inside backend/src
2. Start docker and run powershell script

```
backend/src/Start-InfrastructureLocal.ps1
```
3. Configure visual studio to start multiple projects (Solution -> Properties -> Multiple startup projects): WebApi.Auctionhouse.Command, WebApi.Auctionhouse.Query, WebApi.Auctionhouse.CommandStatus
4. Start backend application (should be in Development hosting environment) 
5. Start frontend application
```
cd front
npx ng serve
```
6. Open http://localhost:4200

## E2E tests

1. When application is started in development environment run
```
cd front
npx cypress open
```
Only the first test (create auction) is currently supported. There is no automated e2e environment in which this test should be running. It's assummed that user with following credentials exists in system:
```
username: test1
password: pass
```

Create auction test
![Alt Text](https://raw.githubusercontent.com/pekalam/auctionhouse/rewrite/docs/img/e2e_create_auction.gif)

