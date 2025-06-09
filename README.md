# Crypto Trading System - Microservices Implementation

This project implements a microservices-based cryptocurrency trading system with two main services: Rates Service and Positions Service.

## Architecture Overview

The system follows Domain-Driven Design (DDD) principles and consists of:

- **Rates Service**: Fetches cryptocurrency rates from CoinMarketCap API and monitors for significant changes
- **Positions Service**: Manages trading positions and calculates profit/loss based on rate changes
- **Event-Driven Communication**: Services communicate through events using MediatR

## Technology Stack

- .NET 8.0
- Entity Framework Core (In-Memory Database)
- MediatR for CQRS and Event Handling
- Minimal APIs
- CsvHelper for CSV parsing
- xUnit for testing

## Prerequisites

- .NET 8.0 SDK
- CoinMarketCap API Key (free tier available)

## Getting Started

### 1. Clone the Repository

```bash
git clone [repository-url]
cd CryptoTradingSystem
```

### 2. Configure API Key

Update the CoinMarketCap API key in `src/Rates/RatesService.API/appsettings.json`:

```json
{
  "CoinMarketCap": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

### 3. Running Locally

**Rates Service:**
```bash
cd src/Rates/RatesService.API
dotnet restore
dotnet run
```

**Positions Service:**
```bash
cd src/Positions/PositionsService.API
dotnet restore
dotnet run
```

## API Endpoints

### Rates Service (Port 5001)

- `POST /api/rates/fetch` - Trigger rate fetching from CoinMarketCap
- `GET /api/rates/recent/{symbol}` - Get recent rates for a symbol

### Positions Service (Port 5002)

- `POST /api/positions` - Add a new position
- `DELETE /api/positions/{id}` - Close a position
- `GET /api/positions` - Get all open positions

## How It Works

1. **Rate Fetching**: The Rates Service fetches current cryptocurrency rates when triggered
2. **Change Detection**: It compares current rates with the oldest rate in the last 24 hours
3. **Event Publishing**: If the change exceeds 5%, a `RateChangedEvent` is published
4. **Position Update**: The Positions Service receives the event and recalculates P&L for affected positions
5. **Result Publishing**: Updated position values are published to a queue (console output in this implementation)

## Project Structure

```
CryptoTradingSystem/
├── src/
│   ├── Positions/           # Positions microservice
│   │   ├── API/            # REST API endpoints
│   │   ├── Application/    # Business logic & handlers
│   │   ├── Domain/         # Domain entities & logic
│   │   └── Infra/          # Data persistence
│   ├── Rates/              # Rates microservice
│   │   ├── API/            # REST API endpoints
│   │   ├── Application/    # Business logic & handlers
│   │   ├── Domain/         # Domain entities & logic
│   │   └── Infra/          # Data persistence
│   ├── Events/             # Shared event contracts
│   └── Messaging/          # Messaging infrastructure
└── Tests/                  # Integration tests
```

## Testing

Run the tests:
```bash
dotnet test
```

## Sample Requests

### Trigger Rate Fetch
```bash
curl -X POST http://localhost:5001/api/rates/fetch
```

### Add Position
```bash
curl -X POST http://localhost:5002/api/positions \
  -H "Content-Type: application/json" \
  -d '{
    "instrumentId": "BTC/USD",
    "quantity": 1.5,
    "initialRate": 50000,
    "side": "BUY"
  }'
```

## License

This project is created as part of a technical assessment.
