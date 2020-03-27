docker run -d -p 5006:8000 --name exercise-daemon-dynamo amazon/dynamodb-local -jar DynamoDBLocal.jar -inMemory -sharedDb 

aws dynamodb create-table --endpoint-url http://localhost:5006 --table-name ExerciseDaemon-Athletes --attribute-definitions AttributeName=Id,AttributeType=N  --key-schema AttributeName=Id,KeyType=HASH --provisioned-throughput ReadCapacityUnits=1,WriteCapacityUnits=1

aws dynamodb list-tables --endpoint-url http://localhost:5006 
