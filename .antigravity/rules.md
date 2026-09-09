# Project Rules: Reservation System (.NET 8 Web API)

- **Framework**: ASP.NET Core Web API (.NET 8)
- **Architecture**: Layered (Controllers -> Services -> Data Access / EF Core)
- **Coding Style**:
  - File-scoped namespaces (`namespace ReservationSystem.API.Models;`)
  - Modern C# features & explicit DTO mapping
  - Async/await for all DB operations (`async`/`await`)
- **Database**: Entity Framework Core (Code-First approach)
