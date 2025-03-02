.PHONY: run

run:
	@echo "Building and running the application using docker-compose..."
	docker-compose up --build -d

stop:
	@echo "Stopping the application..."
	docker-compose down

logs:
	@echo "Fetching logs from the containers..."
	docker-compose logs -f

