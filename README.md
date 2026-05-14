# Overview

BuildTruck Backend provides the server-side services required by the BuildTruck platform. The backend is responsible for exposing REST APIs, managing business logic, handling authentication, connecting with the database, and supporting the communication between the main application modules.

The project follows a service-oriented structure composed of a main backend API and separated microservices for specific domains. This organization improves maintainability and allows each service to focus on a clear business responsibility.

## Backend Services

The backend is organized into the following services:

- **BuildTruck Core API**: contains the main business modules of the platform, such as personnel, materials, machinery, incidents, documentation, notifications, statistics, and configuration.
- **BuildTruck User Service API**: handles authentication, user management, roles, profile information, password management, and user-related operations.
- **BuildTruck Project Service API**: manages project creation, project assignment, project status, manager/supervisor relationships, and project queries.

This structure supports the growth of the platform and allows the team to document, deploy, and validate each service independently.

## API Documentation

The backend services are documented using Swagger/OpenAPI. This allows developers to inspect available endpoints, request parameters, response structures, authentication requirements, and supported HTTP methods.

Deployed Swagger documentation:

- BuildTruck Core API: https://buildtruck-api.duckdns.org/swagger/index.html
- BuildTruck User Service API: https://buildtruck-api.duckdns.org/user-service/swagger/index.html
- BuildTruck Project Service API: https://buildtruck-api.duckdns.org/project-service/swagger/index.html

## Deployment Notes

The backend is deployed in a cloud environment and exposes the services through a public API domain. The deployment uses a reverse proxy configuration to route requests to the corresponding backend service.

Main deployment responsibilities include:

- Exposing the Core API and microservices through public routes.
- Providing Swagger/OpenAPI documentation for each service.
- Supporting authentication using JWT tokens.
- Centralizing service access through a shared backend domain.
- Enabling monitoring and log review through cloud infrastructure tools.

This setup allows the frontend application and development team to interact with the backend services from a deployed environment.

