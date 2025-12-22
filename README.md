<img alt="logo-vengar" src="/docs/gh-logo.png" height="100" width="auto">
# Vengar Network Utilities

A cross platform network utilities application built with C# and Avalonia.
The project focuses on providing practical networking tools with a clean UI, real time feedback, and exportable results.  
It is currently under active and the feature set is expanding continuously.

## Current Features

### Ping Utilities

- Single ping
- Continuous ping with live statistics
  - Sent and received packets
  - Packet loss percentage
  - Average round trip time
- Ping sweep
  - IP range scanning
  - CIDR based scanning

### DNS Utilities

- DNS lookup for hostnames
- Reverse lookup support

### Port Scanner

- Scan multiple ports at once
- Detect open and closed ports
- Designed for fast feedback and simple configuration

### IP Tools

- Detect whether an IP address belongs to a private or public network
- CIDR calculations
- Generate IP ranges from CIDR or subnet masks
- Network information and statistics
  - Network address
  - Broadcast address
  - First and last usable host
  - Total and usable host counts
- IP address representations
  - Decimal
  - Hexadecimal
  - Binary

### Exporting

All tools support exporting results to:
- TXT
- CSV

## Planned Features

The following features are planned and currently being designed or implemented:

- Basic HTTP client
  - Simple REST requests
  - GET, POST, PUT, DELETE
  - Request headers and body support
- WHOIS lookup
- Additional network diagnostic utilities
- Improved statistics and visualizations
- UI and UX refinements

And probably more along the way.

## Architecture

The application follows an MVVM architecture:

- Avalonia for the interface
- CommunityToolkit.Mvvm for state and command management
- Service based design for network operations
- Clear separation between UI, logic, and infrastructure (I hope)

## Project Status

Vengar is massively under development.

APIs, UI, and internal architecture may change frequently as new tools are added and existing ones are refined.  
Stability and completeness are not yet guaranteed, but the project is evolving rapidly.

## Goals

- Provide a unified set of network utilities in a single application
- Keep tools simple, fast, and practical
- Maintain clean code and extensible architecture
- Support exporting and reproducibility of results
- 95%+ test coverage
- Make it cross-platform, but for now I'm targeting UNIX systems.

## Interface (screenshots)

<img alt="" src="/docs/home.png" height="200" width="auto">
<img alt="" src="/docs/ping.png" height="200" width="auto">
<img alt="" src="/docs/portscanner.png" height="200" width="auto">
<img alt="" src="/docs/dns.png" height="200" width="auto">
<img alt="" src="/docs/ip.png" height="200" width="auto">

