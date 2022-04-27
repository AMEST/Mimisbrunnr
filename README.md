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
1. Spaces (personal, private, public)
2. Permissions for spaces (issuing permissions to users (groups _in progress_) to be able to view / edit / delete or administer a space)
3. Enabling anonymous access (permission to read public spaces)
4. Markdown page editor (with the ability to insert html code or disable rendering html inside markdown global in service)
5. Copy / move pages
6. Tree of pages
7. Page updates feed

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
1. In memory - suitable for working in only 1 copy, or for development. (DO NOT RECOMMEND USING THIS TYPE FOR 2 or more service instances)
2. In MongoDB - does not give a special performance boost, but allows you to cache aggregated data without making many queries each time (for example, the space page tree is quite hard to calculate)
3. In Redis - the most productive and recommended mode.

* `Caching:Type` - cache type (`Memory`, `MongoDb`, `Redis`)
* `Caching:RedisConnectionString` - connection string for Redis cache type