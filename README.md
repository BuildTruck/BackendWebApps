# Overview

BuildTruck Backend provides the server-side services required by the BuildTruck platform. The backend is responsible for exposing REST APIs, managing business logic, handling authentication, connecting with the database, and supporting the communication between the main application modules.

The project follows a service-oriented structure composed of a main backend API and separated microservices for specific domains. This organization improves maintainability and allows each service to focus on a clear business responsibility.

## Backend Services

The backend is organized into the following services:

- **BuildTruck Core API**: contains the main business modules of the platform, such as personnel, materials, machinery, incidents, documentation, notifications, statistics, and configuration.
- **BuildTruck User Service API**: handles authentication, user management, roles, profile information, password management, and user-related operations.
- **BuildTruck Project Service API**: manages project creation, project assignment, project status, manager/supervisor relationships, and project queries.

This structure supports the growth of the platform and allows the team to document, deploy, and validate each service independently.
