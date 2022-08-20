[![Mimisbrunnr Build](https://github.com/AMEST/Mimisbrunnr/actions/workflows/main.yml/badge.svg)](https://github.com/AMEST/Mimisbrunnr/actions/workflows/main.yml)
![hub.docker.com](https://img.shields.io/docker/pulls/eluki/mimisbrunnr-wiki.svg)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/amest/Mimisbrunnr)
![GitHub](https://img.shields.io/github/license/amest/Mimisbrunnr)

# Mimisbrunnr - wiki system (like confluence)

## Links
* **[Try Mimisbrunnr](https://wiki.nb-47.ml)**  
* **[Docker image](https://hub.docker.com/r/eluki/mimisbrunnr-wiki)**

## Description

Mímisbrunnr is the source of knowledge. A wiki system for storing knowledge to create a system like atlassian confluence that supports the core features of confluence and recreates a similar user interface for convenience.

### Features

Support for the following features:

- Spaces
  - Create personal, private or public spaces
  - Import pages from **Confluence** when creating new space
  - Configure permissions: allow view / edit / delete pages for individual users or groups
  - Space directory with list user visible spaces
  - Tree of pages
  - Space configuration
- Pages
  - Highlight code blocks on pages
  - Add attachments to pages ( and simplify drag&drop files / images in editor)
  - Markdown page editor (with the ability to insert html code or disable rendering html inside markdown global in service)
  - Copy / move pages in space or between spaces
- Base
  - Page updates feed on home page
  - Recently visited pages
  - Search pages and spaces
  - Multi Language EN or RU
- Profile
  - Profile page with avatar and last worked on
  - Settings _(Not implemented)_
  - Favorites _(Not implemented)_
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
- Hosting
  - Simple scalable
  - Three types of caching (one for single node and two for multiple nodes)
  - Three types of persistent storage (file storage)
  - Prometheus like metrics with PushGateway support

## Screenshots

<div align=center> 

![2022-06-22 23-29-48.gif](https://wiki.nb-47.ml/api/attachment/62b36221590e68370bd76125/2022-06-22%2023-29-48.gif?v=2)

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

* `Persistent:Type` - storage type (`Local`, `GridFs`, `WebDav`)
* Local:
  * `Persistent:Local:Path` - Path in file system where stored files
* GridFs:
  * `Persistent:GridFs:ConnectionString` - MongoDb ConnectionString for gridfs store
* WebDav
  * `Persistent:WebDav:Address` - url to WebDav server (example: `http://nextcloud.local/remote.php/dav/files/sample-username/`)
  * `Persistent:WebDav:Username` - WebDav username
  * `Persistent:WebDav:Password` - WebDav password
  
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
