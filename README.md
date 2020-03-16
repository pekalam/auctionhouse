SQL Server:&nbsp;&nbsp;&nbsp;[![Build Status](https://dev.azure.com/marekbf3/auctionhouse/_apis/build/status/pekalam.auctionhouse?branchName=sqlserver)](https://dev.azure.com/marekbf3/auctionhouse/_build/latest?definitionId=1&branchName=sqlserver)

Eventstore:&nbsp;&nbsp;&nbsp;[![Build Status](https://dev.azure.com/marekbf3/auctionhouse/_apis/build/status/pekalam.auctionhouse?branchName=eventstore)](https://dev.azure.com/marekbf3/auctionhouse/_build/latest?definitionId=1&branchName=eventstore)


**Frontend**

Angular8 + material components

**Backend**

**CQRS and Event sourcing**

![ScreenShot](https://raw.githubusercontent.com/pekalam/auctionhouse/master/docs/img/Arch.png)

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
