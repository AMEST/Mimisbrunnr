[![Mimisbrunnr Build](https://github.com/AMEST/Mimisbrunnr/actions/workflows/main.yml/badge.svg)](https://github.com/AMEST/Mimisbrunnr/actions/workflows/main.yml)
![hub.docker.com](https://img.shields.io/docker/pulls/eluki/mimisbrunnr-wiki.svg)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/amest/Mimisbrunnr)
![GitHub](https://img.shields.io/github/license/amest/Mimisbrunnr)

<div align=center>
	
![apple-touch-icon-180x180.png](https://github.com/AMEST/Mimisbrunnr/raw/main/src/Mimisbrunnr.Web.Host/ClientApp/public/img/icons/android-chrome-192x192.png) 

# Mimisbrunnr - wiki system (like confluence) 

</div>

- [Mimisbrunnr - wiki system (like confluence)](#mimisbrunnr---wiki-system-like-confluence)
  - [Links](#links)
  - [Description](#description)
    - [Features](#features)
  - [Screenshots](#screenshots)
  - [Get started](#get-started)
    - [Configuration](#configuration)
      - [Authentication](#authentication)
      - [Database](#database)
      - [Caching](#caching)
      - [Persistent](#persistent)
      - [Metrics](#metrics)
    - [Deploy](#deploy)
      - [Docker compose (swarm)](#docker-compose-swarm)
  - [Import spaces via cli](#import-spaces-via-cli)
    - [CommandLine arguments](#commandline-arguments)
    - [Example](#example)
  - [Migrating Persistent Storage Data](#migrating-persistent-storage-data)
    - [CommandLine arguments](#commandline-arguments-1)
    - [Example](#example-1)

## Links
* **[Documentation](https://wiki.nb-47.su/space/MM-DOCS)**
* **[Try Mimisbrunnr](https://wiki.nb-47.su)**  
* **[Docker image](https://hub.docker.com/r/eluki/mimisbrunnr-wiki)**
* **[Latest release and compiled binaries](https://github.com/amest/Mimisbrunnr/releases/latest)**

## Description

Mímisbrunnr is the source of knowledge. A wiki system for storing knowledge to create a system like atlassian confluence that supports the core features of confluence and recreates a similar user interface for convenience.

### Features

Support for the following features:

- Spaces
  - Create personal, private or public spaces
  - Import pages from **Confluence** when creating new space or [use cli](#import-spaces-via-cli)
  - Configure permissions: allow view / edit / delete pages for individual users or groups
  - Space directory with list user visible spaces
  - Tree of pages
  - Space configuration
- Pages
  - Highlight code blocks on pages
  - Add attachments to pages ( and simplify drag&drop files / images in editor)
  - Markdown page editor (with the ability to insert html code or disable rendering html inside markdown global in service)
  - Copy / move pages in space or between spaces
  - Save changes to draft when editing a page (allows you to keep changes without publishing them and save changes in case of failure)
  - Page comments
  - Page versioning
- Base
  - Page updates feed on home page
  - Recently visited pages
  - Search pages and spaces
  - Multi Language EN or RU
  - Generating user tokens for token authentication (for direct access to api)
- Profile
  - Profile page with avatar and last worked on
  - Settings (Manage personal api tokens, User Groups, Profile settings _(Not implemented)_, Other settings _(Not implemented)_)
  - Favorites
- Plugins
  - Macros for pages (additional functional for pages)
- Administration
  - Change wiki instance title
  - Enabling anonymous access (permission to read public spaces and access to wiki without authentication)
  - Enable / Disable html in markdown
  - Enable / Disable swagger api documentation
  - Custom home page (use any public space home page as home page wiki instance)
  - Custom css
  - Manage users
    - Promote to admin
    - Demote
    - Disable
    - Enable
  - Manage groups 
  - Manage plugins
    - Install default plugin
    - Install another plugins
    - Disable
    - Enable
    - Remove (with clean all plugin data)
    - Link to simple plugin editor
- Hosting
  - Simple scalable
  - Three types of caching (one for single node and two for multiple nodes)
  - Four types of persistent storage (file storage)
  - Prometheus like metrics with PushGateway support

## Screenshots

<div align=center> 

![preview.gif](https://github.com/AMEST/Mimisbrunnr/raw/main/docs/preview.gif)

</div>    
    
## Get started

### Configuration

All configuration may be configured in `appsettings.json` or in Environment variables

#### Authentication

* `Openid:Authority` - openid authority (example: `https://accounts.google.com`)
* `Openid:ClientId` - client id in openid provider 
* `Openid:ClientSecret` - client secret in openid provider 
* `Openid:ResponseType` - oidc authentication response type (example: `code`)
* `Openid:Scope` - array with oidc scope requested when authenticate. By default in index 0,1,2 values ("openid","profile","email") 

Token authentication:

* `Bearer:SymmetricKey` - symmetric key for jwt token sign
* `Bearer:Issuer` - token issuer (default: `Mimisbrunnr`)
* `Bearer:Audience` - token audience (default: `WebApi`)

#### Database

Mimisbrunnr use MongoDB as persistent data storage.

*  `Storage:ConnectionString` - mongo connection string (example: `mongodb://app:password@localhost/mimisbrunnr?authSource=admin`)

#### Caching

For better responsiveness of the service, caching is used. The service supports several caching modes:
1. In memory - suitable for working in only 1 copy, or for development. _**(DO NOT RECOMMEND USING THIS TYPE FOR 2 OR MORE SERVICE INSTANCES)**_
2. In MongoDB - does not give a special performance boost, but allows you to cache aggregated data without making many queries each time (for example, the space page tree is quite hard to calculate)
3. In Redis - the most productive and recommended mode.

* `Caching:Type` - cache type (`Memory`, `MongoDb`, `Redis`)
* `Caching:RedisConnectionString` - connection string for Redis cache type

#### Persistent

There are several storage options for storing page attachments and other files: 
1. Local file system - only suitable for deployment in a single instance and not in a docker container
2. GridFS - storing files in MongoDB
3. WebDav - storing files in repositories with webdav interface
4. S3 - storing files in S3 compatible service

* `Persistent:Type` - storage type (`Local`, `GridFs`, `WebDav`, `S3`)
* Local:
  * `Persistent:Local:Path` - Path in file system where stored files
* GridFs:
  * `Persistent:GridFs:ConnectionString` - MongoDb ConnectionString for gridfs store
* WebDav
  * `Persistent:WebDav:Address` - url to WebDav server (example: `http://nextcloud.local/remote.php/dav/files/sample-username/`)
  * `Persistent:WebDav:Username` - WebDav username
  * `Persistent:WebDav:Password` - WebDav password
* S3
  * `Persistent:S3:ServiceUrl` - url to s3 compatible service (example: `https://minio.local/`)
  * `Persistent:S3:Bucket` - Bucket name
  * `Persistent:S3:AccessKey` - S3 access key
  * `Persistent:S3:SecretKey` - S3 secret key
  
#### Metrics

To monitor the state of the application, you can enable the collection of metrics.
Metrics are given in prometheus format by default in `/api/metrics` path. Collected metrics: AspNetCore and DotNetRuntime.

* `Metrics:Enabled` - (default `false`) enable/disable collecting metrics and prometheus endpoint (/api/metrics)
* `Metrics:Endpoint` - (default `/api/metrics`) path to prometheus metrics endpoint
* `Metrics:BasicAuth` - (default `false`) enable/disable basic authorization on prometheus endpoint
* `Metrics:Username` - username for basic authorization
* `Metrics:Password` - password for basic authorization

Optional, you can configure PushGateway for send metrics from application to prometheus (if service have dynamic topology).

* `Metrics:PushGatewayEnabled` - (default `false`) enable/disable pushing metrics to PushGateway
* `Metrics:PushGatewayEndpoint` - endpoint where pushing metrics
* `Metrics:PushGatewayJob` - job name for pushed metrics

### Deploy

####  Docker compose (swarm)

```yml
version: '3.8'

services:
  host:
    image: eluki/mimisbrunnr-wiki:main
    environment:
      - "Storage:ConnectionString=mongodb+srv://app:password@mongo-local/mimisbrunnr?retryWrites=true"
      - "Openid:ClientSecret=[Google client secret]"
      - "Openid:ClientId=[google client id]"
      - "Bearer:SymmetricKey=McJmB7dRrfAg9pz6gbSufsds"
      - "Caching:Type=Redis"
      - "Caching:RedisConnectionString=redis-local"
      - "Persistent:Type=GridFs"
      - "Persistent:GridFs:ConnectionString=mongodb+srv://app:password@mongo-local/mimisbrunnr?retryWrites=true"
    ports:
     - target: 80
       published: 80
       protocol: tcp
       mode: host
    deploy:
      replicas: 1
    logging:
      driver: "json-file"
      options:
        max-size: "3m"
        max-file: "3"

  mongo-local:
    image: mongo:5.0.9-focal
    environment:
      - "MONGO_INITDB_ROOT_PASSWORD=password"
      - "MONGO_INITDB_ROOT_USERNAME=app"
    deploy:
      replicas: 1
      resources:
        limits:
          cpus: '0.60'
          memory: 512M
    volumes:
      - /opt/mongoData:/data/db
    logging:
      driver: "json-file"
      options:
        max-size: "3m"
        max-file: "3"

  redis-local:
    image: redis:6.2.4-alpine
    command: 
      - "redis-server"
      - '--maxmemory 120mb'
    deploy:
      replicas: 1
      resources:
        limits:
          memory: 120M
    logging:
      driver: "json-file"
      options:
        max-size: "3m"
        max-file: "3"

```

## Import spaces via cli

The service supports basic import of spaces (pages and attachments) from Atlassian Confluence. This is possible in two ways:
1. In the interface, when creating a space, you can attach an archive with an exported space. But it is not very suitable for large spaces. there may be restrictions on the downloaded archive, as well as this process will be processed by the host, which may adversely affect its performance.
2. Use a special console utility with which you can easily import space of any size, as well as monitor the status of the process by messages in the console.

You can download the latest CLI from the releases page(https://github.com/AMEST/Mimisbrunnr/releases/latest)

### CommandLine arguments

| Argument       | Type   | Description                                  |
| -------------- | ------ | -------------------------------------------- |
| --host=        | string | Base url to Mimisbrunnr Wiki instance        |
| --space=       | string | Space key                                    |
| --token=       | string | Access token for Mimisbrunnr Wiki            |
| --file=        | string | File path to exported zip                    |
| --create-space | flag   | **(Optional)** Need create space when import |
| -h --help      | flag   | **(Optional)** Show help message.            |

### Example

To use the CLI you need:
* Export space from Atlassian Confluence
* Download CLI for your OS
* Issue a personal token in your instance profile Mimisbrunnr Wiki
* Run CLI like below

```
./Mimisbrunnr.DataImport.Cli --host="https://wiki.local" \
     --space=myspace \
     --token="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c" \
     --file="/tmp/Confluence-space-export-160423-660.xml.zip" \
     --create-space
```

## Migrating Persistent Storage Data
Sometimes the size of the instance grows so that the original storage is no longer suitable for storing attachments (for example, when storing attachments locally in 1 node mode or offloading the database server from GridFS, transferring data to a more suitable object storage).   

For this, a tool has been prepared for transferring attachments from one type of storage to another storage

You can download the latest CLI from the releases page(https://github.com/AMEST/Mimisbrunnr/releases/latest)

### CommandLine arguments

| Argument                    | Type   | Description                                           |
| --------------------------- | ------ | ----------------------------------------------------- |
| --connection-string=        | string | MongoDb Connection String                             |
| --from-storage-type=        | string | Persistent type: Local, GridFs, WebDav, S3            |
| --to-storage-type=          | string | Persistent type: Local, GridFs, WebDav, S3            |
| --local-path=               | string | Local storage: Path in file system where stored files |
| --gridfs-connection-string= | string | GridFs storage: MongoDb Connection String             |
| --webdav-address=           | string | WebDav storage: Url to WebDav server                  |
| --webdav-username=          | string | WebDav storage: WebDav username                       |
| --webdav-password=          | string | WebDav storage: WebDav password                       |
| --s3-service-url=           | string | S3 Storage: Url to s3 compatible service              |
| --s3-bucket=                | string | S3 Storage: Bucket name                               |
| --s3-access-key=            | string | S3 Storage: S3 access key                             |
| --s3-secret-key=            | string | S3 Storage: S3 secret key                             |
| --only-absent               | flag   | **(Optional)** Only absent files in target storage    |
| -h --help                   | flag   | **(Optional)** Show help message.                     |

### Example

To use the CLI you need:
* Download CLI for your OS (if migrating not local fs type)
* Download CLI for OS where launched instance of service (if migrating Local FS)
* Run CLI like below

```
./Mimisbrunnr.Migration.Persistent.Cli --connection-string="mongodb+srv://app:password@mongo-local/mimisbrunnr?retryWrites=true" \
    --from-storage-type=GridFs \
    --to-storage-type=S3 \
    --gridfs-connection-string="mongodb+srv://app:password@mongo-local/mimisbrunnr?retryWrites=true" \
    --s3-service-url=https://minio.local \
    --s3-bucket=wiki \
    --s3-access-key=minioAccessKey \
    --s3-secret-key=minioSecretKey
```

