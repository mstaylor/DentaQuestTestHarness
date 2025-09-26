# DentaQuest EDI WCF Test Harness

This project contains the test harness code for connecting to DentaQuest's EDI WCF services with certificate authentication.

## Prerequisites

- .NET 6.0 SDK or later
- Visual Studio Code with C# extension
- Required certificates installed:
  - Client certificate: `bcbsmRealTimeTest` (in Current User > Personal store)
  - Server certificate: `editest.dentaquest.com` or `ediprod.dentaquest.com` (in Current User > Trusted People store)

## Setup

1. Open the project folder in Visual Studio Code
2. Install required NuGet packages: `dotnet restore`
3. Build the project: `dotnet build`

## Running

- Press F5 to run with debugger
- Or use: `dotnet run`

## Configuration

Update the `serviceUrl` in `Program.cs` with the actual DentaQuest service endpoint.

## Notes

- The `EdiWcfRealTimeClient` class is a placeholder - you'll need to generate the actual WCF client proxy from the service reference
- Certificate validation logic automatically selects between production and test environments based on the URL
- Local machine connections use HTTP without certificates for testing