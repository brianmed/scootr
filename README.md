# scootr

Static File Web Server

## Examples

#### All IPs and Port 8080, only HTTP

```bash
$ scootr
```

#### All IPs and Port 8443, only HTTPS

```bash
$ scootr --urls='https://*:8443' --sslCertFile=file.cert --sslKeyFile=file.key
```

#### All IPs and Port 8080 and WebRoot set to /tmp/joy

```bash
$ scootr --weboot=/tmp/joy
```

## Features

- Static Files
- Directory Browser
- HTTP 1.1, HTTTP/2, HTTP/3
- HTTP and HTTPS
- Any IP and Port
- Configurable WebRoot

## Defaults

- Listen on All IPs
- Bind to Port 8080
- WebRoot is Current Working Directory

## Installation

Single [file](https://github.com/brianmed/scootr/releases) deployment.

## Options

```
      --sslCertFile=VALUE    SSL Certificate File (PEM Encoded)
      --sslCertPassword=VALUE
                             SSL Certificate File
      --sslKeyFile=VALUE     SSL Key File (PEM Encoded)
      --urls=VALUE           IP Addresses and Ports to List on [default: http://
                               *:8080]
      --webroot=VALUE        Directory to Serve Static Files from [default: .]
  -h, --help                 Show this Message and Exit
```

## Builidng from Source

```
$ git clone https://github.com/brianmed/scootr.git
$ dotnet publish -c Release --self-contained
```
