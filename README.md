# Payment Gateway API

A payment gateway implementation built with .NET 9, following Clean Architecture and CQRS patterns.

## Overview

This API allows merchants to:
- **Process payments** - Submit card payments and receive Authorized/Declined/Rejected responses
- **Retrieve payments** - Get details of previously processed payments by ID

## Architecture

**Clean Architecture with CQRS:**
- **API Layer** - Controllers, DTOs, Middleware
- **Application Layer** - Commands, Queries, Handlers, Validators (MediatR + FluentValidation)
- **Domain Layer** - Entities, Enums, Interfaces
- **Infrastructure Layer** - Repositories (in-memory), Bank Client (HTTP)

**Key Patterns:**
- CQRS via MediatR
- Repository Pattern
- Dependency Inversion
- Validation Pipeline (FluentValidation)
- Exception Handling Middleware

**Security:**
- Card numbers masked (only last 4 digits stored/returned)
- CVV never persisted
- Idempotent payment processing via Idempotency-Key

## Getting Started

### Prerequisites
- .NET 9 SDK
- Docker Desktop

### Run the Application

```bash
# Start bank simulator
docker-compose up -d

# Run API
cd src/PaymentGateway.Api
dotnet run

# Access Swagger
https://localhost:7092/swagger
```

### Run Tests

```bash
dotnet test
```

## API Endpoints

### POST /api/Payments
Process a payment.

**Request:**
```json
{
  "cardNumber": "2222405343248877",
  "expiryMonth": 4,
  "expiryYear": 2025,
  "currency": "USD",
  "amount": 10000,
  "cvv": "123"
}
```

**Response (200):**
```json
{
  "id": "guid",
  "status": "Authorized",
  "cardNumberLastFour": 8877,
  "expiryMonth": 4,
  "expiryYear": 2025,
  "currency": "USD",
  "amount": 10000
}
```

### GET /api/Payments/{id}
Retrieve payment details.

**Response (200):** Same as POST response

## Testing

**Total: 75+ tests across 3 projects**

- **Domain Tests** (8) - Entities, Value Objects
- **Application Tests** (50+) - Commands, Queries, Validators, Handlers
- **Infrastructure Tests** (15+) - Repository, Bank Client

All tests follow Given-When-Then (BDD) pattern using xUnit, FluentAssertions, and Moq.

## Validation Rules

- **Card Number:** 14-19 numeric characters
- **Expiry Month:** 1-12
- **Expiry Year:** Must be in future
- **Currency:** 3 characters (USD, GBP, EUR)
- **Amount:** Positive integer (minor currency unit)
- **CVV:** 3-4 numeric characters

## Bank Simulator Behavior

- Card ending in **1,3,5,7,9** → Authorized
- Card ending in **2,4,6,8** → Declined
- Card ending in **0** → 503 Error

## Technologies

- .NET 9, ASP.NET Core
- MediatR (CQRS)
- FluentValidation
- xUnit, FluentAssertions, Moq
- Docker
