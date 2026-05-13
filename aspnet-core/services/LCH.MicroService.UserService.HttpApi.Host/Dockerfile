FROM mcr.microsoft.com/dotnet/aspnet:10.0
LABEL maintainer="admin@lch.com"
WORKDIR /app

COPY . /app

EXPOSE 80/tcp
VOLUME [ "./app/Logs" ]
VOLUME [ "./app/Modules" ]

RUN apt update
RUN apt install wget -y

ENTRYPOINT ["dotnet", "LCH.MicroService.UserService.HttpApi.Host.dll"]