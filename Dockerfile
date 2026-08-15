FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WordOfTheDayBot/WordOfTheDayBot.csproj", "./"]
RUN dotnet restore
COPY . .
# For some reason VS doesn't do this, so it was compiling locally but my strict editorconfig rules were making it
# fail in the pipeline. I figure this is a reasonable enough step...
RUN dotnet format
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
ENTRYPOINT ["dotnet", "WordOfTheDayBot.dll"]