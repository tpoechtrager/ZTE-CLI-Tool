
## ZTE-MITM-Proxy ##

### Introduction ###

`zte-mitm-proxy` is a Man-in-the-Middle (MITM) Proxy designed to work as an intermediary between the `ZTE-CLI-Tool` and the ZTE router. The primary purpose of this tool is to workaround TLS issues that occur due to mismatched cipher suites.

### Problem Statement ###

ZTE routers only negotiate AES-CBC cipher suites for TLS connections.

`.NET` relies on `OpenSSL` which has dropped support for `AES-CBC`, considering it insecure.

Because of this discrepancy, making a "secure" connection directly from a `.NET` application to a ZTE router becomes impossible on Linux.

Note: Standard `HTTP` connections are also not an option, as they are rejected by the ZTE router.

### "Solution" ###

`zte-mitm-proxy` leverages GnuTLS to negotiate the deprecated `AES-CBC` cipher suites, acting as a compatibility layer between  `ZTE-Cli-Tool` application and the ZTE router.

### Features ###

* Uses GnuTLS for TLS handshake and secure communication
* Acts as a proxy between ZTE-CLI-Tool and the ZTE router
* Handles secure connection seamlessly

Easy to set up and use

### Requirements ###

* `GnuTLS`
* `gcc`

### Usage ###

    ./zte-mitm-proxy --router-ip <ZTE router IP> --listen-port <Proxy listen port>  

Configure ZTE-CLI-Tool to use the proxy.  
Execute CLI commands as usual.
