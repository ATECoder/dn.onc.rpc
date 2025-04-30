# Changelog
Notable changes to this solution are documented in this file using the 
[Keep a Changelog] style. The dates specified are in coordinated universal time (UTC).

[1.1.9251]: https://github.com/ATECoder/dn.onc.rpc

## [1.1.9251] - 2025-03-29
- Serilog Settings
  - Set log level to warning.
- Tests
  - Remove Console.WriteLine( $"@{methodFullName}" );
  - Replace with $"{methodFullName} initializing" );
  - Reduce reporting of test class initialization.
  - Output the name of the assembly under test.
- Fix incorrect new line escape character.

## [1.1.8535] - 2023-05-15 Preview 202304
* Use cc.isr.Json.AppSettings.ViewModels project for settings I/O.

## [1.1.8518] - 2023-04-28 Preview 01
* Split README.MD to attribution, cloning, open-source and read me files.
* Add code of conduct, contribution and security documents.
* Increment version.

## [1.0.8404] - 2023-01-04
* initial commit.

&copy;  2023 Integrated Scientific Resources, Inc. All rights reserved.

[Keep a Changelog]: https://keepachangelog.com/en/1.0.0/
