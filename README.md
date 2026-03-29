EShop Microservices Platform (.NET Aspire + GenAI)

A modern distributed e-commerce backend built using .NET 8, Aspire, and microservices architecture, enhanced with AI-powered semantic product search using vector embeddings (Qdrant).

Overview

This project demonstrates a real-world backend architecture focusing on scalability, separation of concerns, and cloud-native design.

It is not a simple CRUD application. It showcases:

Microservices with independent data stores 
Distributed system orchestration
Containerized infrastructure
AI-driven search capabilities
Architecture

The system is composed of multiple independent services:

Catalog Service
Handles product data and generates vector embeddings for AI search
Basket Service
Manages user shopping carts using Redis for fast access
Ordering Service
Processes orders with transactional consistency using SQL Server
WebApp (Aggregator)
Acts as a frontend gateway combining multiple services
AppHost (Aspire)
Orchestrates services, dependencies, and observability
AI Integration

The platform integrates vector search to enable intelligent product discovery.

Product data is converted into embeddings
Stored in Qdrant (Vector Database)
Enables:
Semantic search (not just keyword matching)
Similar product recommendations
Context-aware queries
Tech Stack
Backend
.NET 8 / ASP.NET Core
.NET Aspire (Distributed App Host)
Entity Framework Core
Databases
PostgreSQL → Catalog Service
SQL Server → Ordering Service
Redis → Basket Cache
Qdrant → Vector Database
DevOps & Infrastructure
Docker (containerized services)
Aspire Dashboard (service orchestration and monitoring)
Features
Microservices-based architecture
Database-per-service pattern
Service-to-service communication
Distributed orchestration using Aspire
AI-powered semantic product search
Redis caching for performance optimization
Clean architecture and separation of concerns
Getting Started
Prerequisites
.NET 8 SDK
Docker Desktop
Visual Studio 2022 or VS Code
Setup
git clone https://github.com/your-username/eshop-microservices-dotnet-aspire.git
cd eshop-microservices-dotnet-aspire
Run the Application
dotnet run --project AppHost

This will:

Spin up all services via Aspire
Start required containers (Postgres, Redis, SQL Server, Qdrant)
Launch the Aspire dashboard
Key Takeaways

This project demonstrates:

Designing production-ready microservices in .NET
Integrating AI (vector search) into backend systems
Using Aspire for orchestration instead of manual setup
Building scalable, maintainable distributed systems
