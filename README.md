# Picking List PDF Parser

This is a Blazor Web App that allows users to upload a Picking List PDF, parse it using PdfPig, review and correct the extracted data, and save the final version to a database.

## Features

-   **PDF Upload:** Upload picking lists in PDF format.
-   **PDF Parsing:** Uses the `UglyToad.PdfPig` library to extract text content. The parsing logic is a custom-built state machine designed to handle complex, multi-column layouts without relying on AI or OCR.
-   **Review & Edit:** A web interface built with Blazor and Syncfusion components allows users to review the parsed data and make corrections in a rich data grid.
-   **Database Persistence:** Saves the final, approved picking list to a SQLite database using Entity Framework Core.
-   **API Endpoints:** Provides a set of minimal APIs for interacting with the application programmatically.

## Tech Stack

-   .NET 9 / C# 13
-   ASP.NET Core Blazor Web App (Server, Global Interactivity)
-   Entity Framework Core 9
-   Syncfusion Blazor Grids
-   UglyToad.PdfPig
-   xUnit for testing

## Setup and Running the Application

### Prerequisites

-   .NET 9 SDK
-   A Syncfusion license key.

### Instructions

1.  **Clone the repository.**
2.  **Restore Dependencies:**
    ```bash
    dotnet restore
    ```
3.  **Add Syncfusion License Key:**
    Open the `Program.cs` file and replace the placeholder with your Syncfusion license key:
    ```csharp
    // In Program.cs
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
    ```
4.  **Run the application:**
    ```bash
    dotnet run --launch-profile http
    ```
    The application will be available at `http://localhost:5100`. The SQLite database (`pickinglist.db`) will be created automatically in the project root directory when the application starts for the first time.

## API Usage

The application exposes a set of minimal APIs for programmatic access.

### 1. Upload and Parse a PDF

Uploads a PDF file and returns the parsed DTO without saving it to the database.

-   **Endpoint:** `POST /api/pickinglists/upload`
-   **Method:** `POST`
-   **Body:** `multipart/form-data` with a file field named `file`.

**Example `curl` request:**
```bash
curl -X POST -F "file=@/path/to/your/pickinglist.pdf" http://localhost:5100/api/pickinglists/upload
```

### 2. Save a Picking List

Saves a new (or updates an existing) picking list.

-   **Endpoint:** `POST /api/pickinglists`
-   **Method:** `POST`
-   **Body:** A JSON object representing the `PickingListDto`.

**Example `curl` request:**
```bash
curl -X POST -H "Content-Type: application/json" -d '{ "salesOrderNumber": "SO123", "items": [ ... ] }' http://localhost:5100/api/pickinglists
```

### 3. Get Picking List by ID

Retrieves a picking list by its unique database ID.

-   **Endpoint:** `GET /api/pickinglists/{id}`
-   **Method:** `GET`

**Example `curl` request:**
```bash
curl http://localhost:5100/api/pickinglists/1
```

### 4. Get Picking List by Sales Order Number

Retrieves a picking list by its unique Sales Order number.

-   **Endpoint:** `GET /api/pickinglists?soNumber={soNumber}`
-   **Method:** `GET`

**Example `curl` request:**
```bash
curl "http://localhost:5100/api/pickinglists?soNumber=SO123"
```

## Screenshots

***Note:*** *Frontend verification could not be completed due to environmental constraints that prevented the Blazor application from running in a stable manner required for automated UI testing. The screenshots for the Upload, Review, and Details pages would be displayed here.*
