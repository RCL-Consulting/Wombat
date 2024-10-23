Wombat - Work-Based Assessment Tool
===================================

**Wombat** is a web-based application built using **ASP.NET Core** that supports the management of **work-based assessments (WBA)** for medical specialists using the **EPA (Entrustable Professional Activities)** approach. It is designed to be highly configurable, supporting multiple institutions, specialities, and subspecialities. The application is hosted on **Azure**, with a backend powered by **MS SQL**, also deployed in Azure.

Features
--------

*   **Multi-Institution Support**: Configure and manage assessments across multiple institutions.
*   **Customizable Specialties and Subspecialties**: Add and manage specialities and subspecialities, along with their corresponding EPAs.
*   **User Roles**:
    *   **Admin**: Full system control.
    *   **Institution Coordinator**: Oversees institution-specific configurations and reviews trainee portfolios.
    *   **Assessor**: Responds to assessment requests from trainees.
    *   **Trainee**: Sends assessment requests and manages their ePortfolio.
*   **Customizable Assessment Forms**:
    *   Configurable questions with either text-based responses or multiple-choice options.
    *   Assessment forms can be tied to specific EPAs.
*   **Assessment Workflow**:
    *   Trainees submit assessment requests to a specific assessor.
    *   Assessors complete the assessment, which is added to the trainee’s ePortfolio.
    *   Trainees can exclude certain assessments from their portfolios if desired.
    *   Institution Coordinators can review the completed portfolios.

Technology Stack
----------------

*   **Frontend**: ASP.NET Core
*   **Backend**: MS SQL (Azure)
*   **Hosting**: Azure App Services
*   **Database**: Azure SQL Database

Installation
------------

### Prerequisites

*   .NET Core SDK (v6 or later)
*   SQL Server or Azure SQL Database
*   Azure Account (for deployment)

### Setup Instructions

1.  Clone the repository:

    ```bash
    git clone https://github.com/your-username/wombat.git
    cd wombat
    ```    
1.  Set up the database:
    
    *   Create a SQL Server or Azure SQL Database instance.
    *   Run the provided SQL scripts (located in the `/scripts` folder) to set up the database schema.
2.  Configure connection strings:
    
    *   Update the connection string in `appsettings.json` to point to your database.
    
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=your-server.database.windows.net;Database=wombat_db;User Id=your-username;Password=your-password;"
      }
    }
    ```
    
3.  Run the application locally:
    
    ```bash  
    dotnet run
    ```
    
4.  Deploy to Azure:
    
    *   Follow the official Microsoft guide to deploy an ASP.NET Core web app to Azure: [Deploy an ASP.NET Core web app to Azure](https://docs.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore).

Usage
-----

1.  **Admin Panel**: Log in as an admin to configure institutions, specialities, and user roles, configure EPAs and assessment forms.
2.  **Coordinator**: Review trainee portfolios.
3.  **Assessor**: Respond to assessment requests from trainees and complete forms.
4.  **Trainee**: Request assessments, view, manage, and curate your ePortfolio.

Contributing
------------

We welcome contributions! Please follow these steps:

1.  Fork the repository.
2.  Create a new feature branch (`git checkout -b feature-branch`).
3.  Commit your changes (`git commit -m "Add new feature"`).
4.  Push to the branch (`git push origin feature-branch`).
5.  Create a new Pull Request.

License
-------

Wombat is licensed under the GPL-3.0 license.
