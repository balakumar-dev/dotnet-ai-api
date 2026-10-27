# Inventory API Architecture

The Inventory API is a sample backend described for this learning project.

Architecture:
- ASP.NET Core Web API
- Clean Architecture with API, Core, and Infrastructure
- JWT authentication for protected inventory endpoints
- PostgreSQL as the application database

The Inventory API stores products, stock quantities, and warehouse locations in PostgreSQL. It does not use SQL Server.

When a client asks "What database does the application use?", the answer from these documents is PostgreSQL.
