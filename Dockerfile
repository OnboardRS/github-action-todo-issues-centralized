FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
ARG GITHUB_ACTOR
ARG GITHUB_TOKEN

WORKDIR /build

COPY ./nuget-template.config ./nuget.config

COPY ./Onboard.MarketingMaterials.Api/Onboard.MarketingMaterials.Api.csproj ./Onboard.MarketingMaterials.Api/Onboard.MarketingMaterials.Api.csproj
COPY ./Onboard.MarketingMaterials.Business/Onboard.MarketingMaterials.Business.csproj ./Onboard.MarketingMaterials.Business/Onboard.MarketingMaterials.Business.csproj
COPY ./Onboard.MarketingMaterials.Common/Onboard.MarketingMaterials.Common.csproj ./Onboard.MarketingMaterials.Common/Onboard.MarketingMaterials.Common.csproj
COPY marketing-materials.sln .

RUN sed -i -e "s/ACTOR/$GITHUB_ACTOR/g" -e "s/TOKEN/$GITHUB_TOKEN/g" nuget.config

RUN dotnet restore marketing-materials.sln

COPY . .

RUN dotnet publish marketing-materials.sln -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime 
WORKDIR /api 

COPY --from=build /publish .

CMD ["dotnet", "Onboard.MarketingMaterials.Api.dll"]

#NOTE: Build Image Command 
# docker build -t onboard-contentful-api-local —build-arg GITHUB_ACTOR=[GITUB_USER] —build-arg GITHUB_TOKEN=[GITHUB_TOKEN] .

#For Windows:
# docker container prune -f
# docker run `
# -e AWS_PROFILE=sandbox `
# -e AWS_REGION=us-west-2 `
# -e CONTENTFUL_SECRET_MANAGER_KEY=sandbox/contentfulsecrets `
# -e CONTENTFUL_ENVIRONMENT=dev `
# -e CONTENTFUL_MANAGEMENT_BASE_URL=https://api.contentful.com `
# -p 8080:80 `
# --name onboard-contentful-api-local `
# -v $env:USERPROFILE\.aws:/root/.aws:ro `
# onboard-contentful-api-local
