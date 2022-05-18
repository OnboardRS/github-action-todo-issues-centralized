# Set the base image as the .NET 6.0 SDK (this includes the runtime)
FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

# Copy everything and publish the release (publish implicitly restores and builds)
WORKDIR /app
COPY . ./
RUN dotnet publish ./OnboardRS.ToDoIssues.GitHubAction/OnboardRS.ToDoIssues.GitHubAction.csproj -c Release -o out --no-self-contained

# Label the container
LABEL maintainer="OnboardRS"
LABEL repository="https://github.com/OnboardRS/github-action-todo-issues-centralized"
LABEL homepage="https://github.com/OnboardRS/github-action-todo-issues-centralized"

# Label as GitHub action
LABEL com.github.actions.name="TODO Actions Centralized"
# Limit to 160 characters
LABEL com.github.actions.description="Creates github issues when the exact phrase // TODO: is found. Stores the issues in a centralized repo for easier project planning within an organization."
# See branding:
# https://docs.github.com/actions/creating-actions/metadata-syntax-for-github-actions#branding
LABEL com.github.actions.icon="activity"
LABEL com.github.actions.color="orange"

# Relayer the .NET SDK, anew with the build output
FROM mcr.microsoft.com/dotnet/sdk:6.0
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "/OnboardRS.ToDoIssues.GitHubAction.dll" ]