# Task Management Application

A .NET 9.0 Razor Pages application for task management with SQLite database.

## Deployment on Render

### Prerequisites

- A GitHub repository with your code
- A [Render](https://render.com) account

### Steps to Deploy

1. Push your code to GitHub including the Dockerfile and docker-compose.yml

2. Log in to your Render account and create a new Web Service:
   - Connect your GitHub repository
   - Select "Docker" as the environment
   - Set a name for your service
   - Choose the branch to deploy
   - Set the following configurations:
     - Region: Choose the closest to your users
     - Instance Type: Free or paid tier based on your needs
     - Environment Variables (optional):
       - ASPNETCORE_ENVIRONMENT: Production

3. Click "Create Web Service"

### Database Persistence

This application uses SQLite. On Render, to persist the database:

1. Create a Disk resource in Render
2. Attach it to your Web Service
3. Mount it to the /app/Data path

## Running Locally with Docker

```bash
# Build and start the application
docker-compose up -d

# Access the application at http://localhost:10000

# Stop the application
docker-compose down
```

## Notes for Deployment

- The application runs on port 10000 by default
- SQLite database is stored in the application directory
- For production, consider using environment variables for sensitive information 