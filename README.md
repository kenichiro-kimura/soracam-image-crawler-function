# SoracamImageCrawlerFunction

## Abstract

This is an Azure Functions project for crawling images from Soracam.

This application launch every five minutes by defauld, and get a current image from specified Soracam device, and upload it to Azure  Blob Storage.

You can check the image with [blobtrigger-azure-function-tensorflow](https://github.com/kenichiro-kimura/blobtrigger-azure-function-tensorflow) or as you like.

## How to use

1. Create a Soracom API SDK from OpenAPI document

```bash
docker run --rm -v ${PWD}:/local openapitools/openapi-generator-cli generate -i /local/soracom-api.ja.yaml -g csharp-netcore -o /local
```

2. Create an Azure Function App and Storage Account
3. Create a Blob Container named `soracom`
4. Get a connection string of the Storage Account
5. Add configurtations to Function App settings as follows

| Key | Value |
| --- | --- |
| ApplicaitonOptions__SoraCamDeviceId | Your sora_cam device id to crawl images |
| ApplicaitonOptions__BlobContainerName | Blob container name to upload crawl images |
| SoracomOptions__AuthKeyId | Your Soracom API key id |
| SoracomOptions__AuthKey | Your Soracom API key |

If you want to authenticate with a mail address and password, add the following settings.

| Key | Value |
| --- | --- |
| SoracomOptions__Email | Email address of Soracom root account |
| SoracomOptions__Password | Password of Soracom root account |

If you want to authenticate with a SAM user, add the following settings.

| Key | Value |
| --- | --- |
| SoracomOptions__UserName | User name of SAM user |
| SoracomOptions__OperatorId | Operator ID of SAM user |
| SoracomOptions__Password | Password of SAM user |

And create a connection string named `StorageConnectionString` with the connection string of the Storage Account.

6. Build and deploy the project to the Function App
