.PHONY: setup
setup:
	docker-compose build

.PHONY: build
build:
	docker-compose build lbh-fss-step-function

.PHONY: test
test:
	docker-compose up test-database & docker-compose build lbh-fss-step-function-test && docker-compose up lbh-fss-step-function-test