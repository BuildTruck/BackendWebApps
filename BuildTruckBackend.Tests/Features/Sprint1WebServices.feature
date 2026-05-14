# BuildTruck Sprint 1 backend web-service scenarios.
# This file documents BDD-style integration and acceptance scenarios for Auth + Users and Projects.

@sprint1 @auth-users
Feature: Auth and Users Web Services
  Como usuario de BuildTruck
  Quiero autenticarme y gestionar la informacion de mi perfil
  Para acceder de forma segura a las funcionalidades del sistema

  Scenario: Successful login with valid credentials
    Given que existe un usuario registrado con correo y password validos
    When el usuario envia una solicitud POST al endpoint de login
    Then el sistema debe retornar una respuesta exitosa
    And la respuesta debe incluir un token JWT
    And la respuesta debe incluir la informacion basica del usuario

  Scenario: View and update profile data
    Given que existe un usuario autenticado en el sistema
    When se actualizan los datos permitidos del perfil
    Then el sistema debe conservar la identidad del usuario
    And debe reflejar los nuevos datos personales
    And debe mantener las reglas de generacion de correo corporativo
