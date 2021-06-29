FROM aemari/spacer-prose-backend:latest

COPY ./SpacerTransformationsAPI /SpacerProseBackend
RUN echo $(ls /SpacerProseBackend)
WORKDIR /SpacerProseBackend/SpacerTransformationsAPI/
RUN echo $(ls)
#build dotnet stuff
RUN dotnet build

ENTRYPOINT dotnet run
