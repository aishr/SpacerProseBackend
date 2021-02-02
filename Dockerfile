FROM aishr/spacer-prose-backend:latest

#dynamodb iam creds
ENV TABLE_NAME "spacer-visualization"
ENV ACCESS_KEY_ID AKIASOHWIFLXIIGPYLSH
ENV SECRET_ACCESS_KEY 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
ENV REGION_NAME "us-east-2"

COPY ./SpacerTransformationsAPI /SpacerProseBackend
RUN echo $(ls /SpacerProseBackend)
WORKDIR /SpacerProseBackend/SpacerTransformationsAPI/
RUN echo $(ls)
#build dotnet stuff
RUN dotnet build

ENTRYPOINT dotnet run
