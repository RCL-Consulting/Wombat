![License](https://img.shields.io/github/license/reniercloete/Wombat)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/reniercloete/Wombat)
![GitHub last commit](https://img.shields.io/github/last-commit/reniercloete/Wombat)

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
*   [Visual Studio community](https://visualstudio.microsoft.com/vs/community/)

### Setup Instructions

1.  Clone the repository:

    ```bash
    git clone https://github.com/your-username/wombat.git
    cd wombat
    ```    
1.  Set up the database:

    To run Wombat on the development machine, simply run `update-database` in the package manager console and you should be ready to go.

    To run Wombat on Azure, 
    *   Create a SQL Server or Azure SQL Database instance and create a new database [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio) works well.
    *   Generate a create script using `Script-Migration -Idempotent` in the package manager console and run the SQL script on your newly created database.
      
3.  Configure connection strings:
    
    *   Update the connection string in `appsettings.json` to point to your database.
    
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=your-server.database.windows.net;Database=wombat_db;User Id=your-username;Password=your-password;"
      }
    }
    ```    
4.  Run the application locally:
    
    ```bash  
    dotnet run
    ```
    
5.  Deploy to Azure:
    
    *   Follow the official Microsoft guide to deploy an ASP.NET Core web app to Azure: [Deploy an ASP.NET Core web app to Azure](https://docs.microsoft.com/en-us/azure/app-service/quickstart-dotnetcore).

Usage
-----

The repo is pre-populated with some users and data to get things going as fast as possible:

1.  **Admin Panel**: Log in as an admin to configure institutions, specialities, and user roles, configure EPAs and assessment forms. Log in as `admin@localhost.com` with password `P@ssw0rd`.
2.  **Coordinator**: Review trainee portfolios. Log in as `coordinator@localhost.com` with password `P@ssw0rd`.
3.  **Assessor**: Respond to assessment requests from trainees and complete forms.  Log in as `assessor@localhost.com` with password `P@ssw0rd`.
4.  **Trainee**: Request assessments, view, manage, and curate your ePortfolio.  Log in as `trainee@localhost.com` with password `P@ssw0rd`.

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

Wombat is licensed under the GNU Affero GPL-3.0 license.
