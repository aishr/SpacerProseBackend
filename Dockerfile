FROM ubuntu:18.04
RUN apt update && apt install -y wget
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O /opt/packages-microsoft-prod.deb && dpkg -i /opt/packages-microsoft-prod.deb
#install dotnet and other stuffs
RUN apt update && apt install -y vim  apt-transport-https dotnet-sdk-3.1

#dynamodb iam creds
ENV TABLE_NAME "spacer-visualization"
ENV ACCESS_KEY_ID AKIASOHWIFLXIIGPYLSH
ENV SECRET_ACCESS_KEY 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
ENV REGION_NAME "us-east-2"

COPY ./SpacerTransformationsAPI /SpacerProseBackend

WORKDIR ./SpacerProseBackend/SpacerTransformationsAPI/
#build dotnet stuff
RUN dotnet build

ENTRYPOINT dotnet run
