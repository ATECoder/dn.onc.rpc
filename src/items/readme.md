# About

[ISR's ONC RPC] is a C# implementation of the [Java ONC RPC] implementation called Remote Tea.

## History

[ISR's ONC RPC] is a fork of [GB1.RemoteTea.Net], which was forked from [Wes Day's GitHub repository], which is a fork of 
[Jay Walter's SourceForge repository], which is a port of [Java ONC RPC].

## Standards

* [XDR: External Data Representation Standard (May 2006)]
* Open Network Computing Remote Procedure Call (ONC RPC)
	* [RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]
	* [Binding Protocols for ONC RPC Version 2 (August 1995)]

## How to Use

For some clues on using [ISR's ONC RPC] see the [ISR's ONC RPC] MS Test project.

## Departures from [GB1.RemoteTea.Net]

* The XDR classes were moved to the [ISR's XDR] package;
* The base namespace was changed from org.acplt to cc.isr;
* The ONC/RPC namespace was changed from org.acplt.oncrpc to cc.isr.ONC.RPC;
* The casing of namespace suffices, such as server and web, were changed to Pascal.
* Pascal case naming convention is used for classes, methods and properties;
* Interface names are prefixed with 'I';
* Base class names are suffixes with Base;
* the xdrAble interface was renamed to IXdrCodec;
* The xdr prefixes were removed from the codec methods;
* Uppercase constant names were converted to Pascal casing while retaining the original constant names in the code documentation;
* The namespace of the ONC/REPC client classes was changed fro isr.cc.ONC.RPC.Clients;
* Getters and setters, such as Get and Set Character Encoding, where changed to properties where possible.
* Static constant classes were converted to Enum constructs.
* Renamed OncRpcCallInformation to OncRpcCallHandler.
* Renamed OncRpcServerTransportRegistrationInfo to OncRpcProgramInfo.


## Feedback

[ISR's ONC RPC] is released as open source under the MIT license.
Bug reports and contributions are welcome at the [ISR's ONC RPC] repository.

[ISR's ONC RPC]: https://github.com/ATECoder/dn.onc.rpc

[XDR: External Data Representation Standard (May 2006)]: http://tools.ietf.org/html/rfc4506
[RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]: http://tools.ietf.org/html/rfc5531
[Binding Protocols for ONC RPC Version 2 (August 1995)]: http://tools.ietf.org/html/rfc1833

[Jay Walter's SourceForge repository]: https://sourceforge.net/p/remoteteanet
[Wes Day's GitHub repository]: https://github.com/wespday/RemoteTea.Net
[GB1.RemoteTea.Net]: https://github.com/galenbancroft/RemoteTea.Net
[org.acplt.oncrpc package]: https://people.eecs.berkeley.edu/~jonah/javadoc/org/acplt/oncrpc/package-summary.html
[Java ONC RPC]: https://github.com/remotetea/remotetea/tree/master/src/tests/org/acplt/oncrpc

