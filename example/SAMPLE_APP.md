# Running the LiveDocs Sample App

This guide will walk you through running the LiveDocs sample application, which demonstrates the capabilities of RxDBDotNet in a real-world scenario using .NET Aspire.

## Prerequisites

Before you begin, ensure you have the following installed:

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
2. [Docker Desktop](https://www.docker.com/products/docker-desktop) or a compatible container service
3. [Node.js](https://nodejs.org/) (version 18 or later)

## Steps to Run the Sample App

1. **Clone the Repository**

   If you haven't already, clone the RxDBDotNet repository:

   ```bash
   git clone https://github.com/Ziptility/RxDBDotNet.git
   cd RxDBDotNet
   ```

2. **Start Docker**

   Ensure Docker Desktop (or your compatible container service) is running.

3. **Build and Run the .NET Aspire Application**

   Navigate to the AppHost project directory:

   ```bash
   cd examples/LiveDocs.AppHost
   ```

   Run the application:

   ```bash
   dotnet run
   ```

   This will start the .NET Aspire application, including the GraphQL API.

4. **Run the RxDB Client**

   Open a new terminal window, navigate to the RxDB client directory:

   ```bash
   cd examples/LiveDocs.RxDBClient
   ```

   Install dependencies and start the client:

   ```bash
   npm install
   npm start
   ```

5. **Access the Application**

   Open your web browser and navigate to `http://localhost:1337`. You should see the LiveDocs Heroes application running.

## Using the Application

- Add new heroes using the form at the bottom of the page.
- Delete heroes by clicking the DELETE button next to each hero.
- Observe real-time updates across multiple browser tabs or windows.

## Important Note

The server-side repository in this example is in-memory only. If you stop the app, the server-side data will not be in sync (or be the "source of truth") for the state of the system. The client might have more documents cached locally in their local storage that the server is unaware of upon restart. Therefore, you cannot delete documents at that point that were created during the last run.

For a production scenario, you would typically use a persistent storage solution to maintain data consistency across restarts.

## Troubleshooting

If you encounter any issues:

1. Ensure all prerequisites are correctly installed.
2. Check that Docker is running.
3. Verify that ports 1337 (for the client) and the ports used by .NET Aspire are not in use by other applications.

For more detailed information about the implementation, refer to the source code in the `examples/LiveDocs` directory.

