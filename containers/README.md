# List of container images and how to use them

1. AAI-Api
   - **Description**: A REST API for interacting with the AAI Copilot.
   - **Usage**: 
     - Start the container using `docker run -p 8080:8080 aai-rest-copilot-apis`.
     - Access the API at `http://localhost:8080`.
2. AAI-MCP
   - **Description**: A MCP server running the tools and resources for the AAI Copilot.
   - **Usage**: 
     - Start the container using `docker run -p 8081:8080 aai-mcp-apis`.
     - Access the API at `http://localhost:8081`.
3. AAI-Web
   - **Description**: A web application for the AAI Copilot.
   - **Usage**: 
     - Start the container using `docker run -p 8082:8080 aai-web`.
     - Access the web application at `http://localhost:8082`.

## Environment Variables

For application configuration, you can set the following environment variables:

TBD