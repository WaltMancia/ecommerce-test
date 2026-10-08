# Arquitectura y Trust Boundaries

Este documento define las bases para el DFD (Data Flow Diagram).

## Entidades Externas

- **Customer:** Usuario desde navegador web (Internet).
- **Admin:** Usuario con privilegios (Internet).
- **Payment Gateway Simulator:** Representa un procesador externo (Ej. Stripe).
- **Notification Service Simulator:** Representa un proveedor SMTP/SMS.

## Trust Boundaries

1. **Internet Boundary:** Entre el navegador del usuario y el Frontend.
2. **API Boundary:** Entre el Frontend y el Backend API (Validación JWT, Rate Limiting).
3. **Database Boundary:** Entre el Backend API y MySQL (Solo la API tiene acceso, red interna).
4. **External Services Boundary:** Entre el Backend API y los Simuladores.
