.PHONY: setup
setup:
	docker-compose build

.PHONY: test
test:
	docker-compose up test-database & docker-compose build lbh-fss-step-function-test && docker-compose up lbh-fss-step-function-test

.PHONY: test-cci
test-cci:
	docker-compose run lbh-fss-step-function-test
