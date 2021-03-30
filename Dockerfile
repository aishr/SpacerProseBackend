FROM ubuntu:18.04
RUN apt update && apt install -y wget
RUN wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O /opt/packages-microsoft-prod.deb && dpkg -i /opt/packages-microsoft-prod.deb
#install dotnet and other stuffs
RUN apt update && apt install -y vim  apt-transport-https dotnet-sdk-5.0

COPY ./SpacerTransformationsAPI /SpacerProseBackend
RUN echo $(ls /SpacerProseBackend)
WORKDIR /SpacerProseBackend/SpacerTransformationsAPI/
RUN echo $(ls)
#build dotnet stuff
RUN dotnet build

ENTRYPOINT dotnet run
