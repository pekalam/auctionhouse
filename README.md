(2022 update)\
**!!Work in progress - some functionalities are not working!!!**\

# Introduction

Fake web shop that provides functionality such as buying items, selling items and creating auctions. 
It is created using CQRS and event sourcing patterns and it is using ports and adapters architecture. System is split into query and command operations that are reading and writing from separate databases. Query database is being synchronized with command database by using event bus.

## C4 Container diagram

![ScreenShot](https://raw.githubusercontent.com/pekalam/auctionhouse/rewrite/docs/img/Auctionhouse_C4_2.png)


