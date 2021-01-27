FROM mcr.microsoft.com/dotnet/core/sdk:3.1

# disable microsoft telematry
ENV DOTNET_CLI_TELEMETRY_OPTOUT='true'

WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.config ./
COPY ./lbh-fss-step-function.sln ./
COPY ./FSSStepFunction/LbhFssStepFunction.csproj ./FSSStepFunction/
COPY ./LbhFssStepFunction.Tests/LbhFssStepFunction.Tests.csproj ./LbhFssStepFunction.Tests/

RUN dotnet restore ./FSSStepFunction/LbhFssStepFunction.csproj
RUN dotnet restore ./LbhFssStepFunction.Tests/LbhFssStepFunction.Tests.csproj

# Copy everything else and build
COPY . .

RUN dotnet build -c debug -o out LbhFssStepFunction.Tests/LbhFssStepFunction.Tests.csproj

CMD dotnet test
