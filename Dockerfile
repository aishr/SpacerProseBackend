FROM ubuntu:18.04
RUN apt update && apt install -y wget
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O /opt/packages-microsoft-prod.deb && dpkg -i /opt/packages-microsoft-prod.deb
#install dotnet and other stuffs
RUN apt update && apt install -y vim  apt-transport-https dotnet-sdk-3.1

COPY ./SpacerTransformationsAPI /SpacerProseBackend

#build dotnet stuff
RUN cd /SpacerProseBackend/SpacerTransformationsAPI && dotnet build && dotnet run
