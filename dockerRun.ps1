docker container prune -f
docker run `
-e AWS_REGION=us-west-2 `
-p 8080:80 `
--name todo-actions-centralized `
-v $env:USERPROFILE\.aws:/root/.aws:ro `
todo-actions-centralized