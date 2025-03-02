#Known issues
when you start it via docker compose the executor and the api container starts when the rabbtiMQ is still setting up, if this is the first start then the postgres as well, you may see some errors as these 
containers are failing to connect. When the rabbit and the db gets set up the errors will dissapear but because of this issue after you start the service you need to wait a couple of seconds before the API can consume your requests
after that everything should work fine.

# How would you deploy and scale the service?

## Kubernetes
To handle scaling and deployment in production
we can also scale the API and BackgroundService horizontally (by increasing the number of pods in Kubernetes), 
depending on the workload. RabbitMQ can be scaled by adding more nodes if needed
## CI/CD Pipeline
A Continuous Integration/Continuous Deployment (CI/CD) pipeline can be set up using tools like GitHub Actions, GitLab CI, or Jenkins to automate testing, building, and deployment.

# What database solution would you use in production vs. local development?
## Production Database
using a fully managed PostgreSQL service. Services like Amazon RDS, Azure PostgreSQL, or Google Cloud SQL are scalable, highly available, and come with automatic backups and maintenance.
## Local Development
running PostgreSQL in a Docker container is a good approach, as it mimics the production setup

# How and what would you monitor?

API and BackgroundService:
	Request metrics (response times, error rates).
	Throughput (requests per second).
	CPU, Memory usage, and Disk I/O.

RabbitMQ:
	Queue depth (number of pending messages).
	Consumer/Producer rates (how fast messages are consumed and produced).
	Message delivery time.

PostgreSQL:
	Query performance (slow queries, execution times).
	Connection pool usage.
	
# What further changes would you make?
Rate Limiting/Throttling
Retry Logic and Dead-letter Queues (DLQs)
Security enchantments as right now there are no checks on the incoming commands

# What kind of tests would you implement?
Unit Tests: for individual components
Integration Tests: to test how the components interact with each other. We can use a real or in-memory database for this, as well as a local RabbitMQ instance (or Dockerized service).
End-to-End (E2E) Tests: Implement tests that simulate real user interactions with the entire system. For instance, test creating a task via the API and ensure that it gets picked up by the BackgroundService and processed correctly.
Performance Tests: load and stress tests
CI Pipeline Integration: Ensure your tests run automatically as part of the CI/CD pipeline, and that only tested and verified code is deployed.
Contract Tests: for this we can use tools like Pact

# start/stop
you can use docker-compose up --build -d to build 
to shut it down you can use docker-compose down
if you want to take it down with the volumes then 
docker-compose down --volumes

#CURL
## get all
curl --request GET \
  --url http://localhost:3500/api/tasks \
  --header 'User-Agent: Insomnia/2023.5.6'

## get at
curl --request GET \
  --url 'http://localhost:3500/api/tasks/894d26d1-3e2a-4abb-9641-37d087b3d31d?id=e144c0ef-96c2-4f7d-8425-726134f152e9' \
  --header 'Content-Type: application/json' \
  --header 'User-Agent: Insomnia/2023.5.6'

## add task
curl --request POST \
  --url http://localhost:3500/api/tasks \
  --header 'Content-Type: application/json' \
  --header 'User-Agent: Insomnia/2023.5.6' \
  --data '{
  "command": "echo hello"
}'