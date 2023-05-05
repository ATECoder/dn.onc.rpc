### ONC RPC

Implementation of Sun's ONC/RPC Remote Procedure Protocol, including client and server functionality and some associated tools.

* [Description](#Description)
  * [History](#History)
  * [Standards](#Standards)
* [Runtime Pre-Requisites](#Runtime-Pre-Requisites)
* [Known Issues](#Known-Issues)
* Project README files:
  * [cc.isr.ONC.RPC](/src/rpc/oncrpc/readme.md) 
* [Attributions](Attributions.md)
* [Change Log](./CHANGELOG.md)
* [Cloning](Cloning.md)
* [Code of Conduct](code_of_conduct.md)
* [Contributing](contributing.md)
* [Legal Notices](#legal-notices)
* [License](LICENSE)
* [Open Source](Open-Source.md)
* [Repository Owner](#Repository-Owner)
* [Security](security.md)
	
#### Description

[ISR's ONC RPC] is a C# implementation of the [Sun RPC] ported from the [Java ONC RPC] implementation termed Remote Tea.

#### History

[ISR's ONC RPC] is a fork of [GB1.RemoteTea.Net], which was forked from [Wes Day's RemoteTea.Net], which is a fork of [Jay Walter's SourceForge repository], which is a port of [Java ONC RPC] as documented in [org.acplt.oncrpc package].

[ISR's ONC RPC] uses [ISR's XDR], which is a C# implementation of the [XDR: External Data Representation Standard (May 2006)] as implemented in [Java ONC RPC] implementation called Remote Tea. [ISR's XDR] was split off from [GB1.RemoteTea.Net].

[ISR's ONC RPC] is used in [ISR's VXI-11], which is a partial C# implementation of the [VXI Bus Specification].

[ISR's VXI-11] is a fork of [VXI11.CSharp].

##### Standards

* [XDR: External Data Representation Standard (May 2006)]
* Open Network Computing Remote Procedure Call (ONC RPC)
	* [RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]
	* [Binding Protocols for ONC RPC Version 2 (August 1995)]

<a name="Repository-Owner"></a>
#### Repository Owner
[ATE Coder]

<a name="Authors"></a>
#### Authors
* [ATE Coder]  

<a name="Legal-Notices"></a>
#### Legal Notices

Integrated Scientific Resources, Inc., and any contributors grant you a license to the documentation and other content
in this repository under the [Creative Commons Attribution 4.0 International Public License](https://creativecommons.org/licenses/by/4.0/legalcode), see the [LICENSE](LICENSE) file, and grant you a license to any code in the repository under the [MIT License](https://opensource.org/licenses/MIT), see the [LICENSE-CODE](LICENSE-CODE) file.

Integrated Scientific Resources, Inc., and/or other Integrated Scientific Resources, Inc., products and services referenced in the documentation may be either trademarks or registered trademarks of Integrated Scientific Resources, Inc., in the United States and/or other countries. The licenses for this project do not grant you rights to use any Integrated Scientific Resources, Inc., names, logos, or trademarks.

Integrated Scientific Resources, Inc., and any contributors reserve all other rights, whether under their respective copyrights, patents, or trademarks, whether by implication, estoppel or otherwise.

[Creative Commons Attribution 4.0 International Public License]:(https://creativecommons.org/licenses/by/4.0/legalcode)
[MIT License]:(https://opensource.org/licenses/MIT)
 
[ATE Coder]: https://www.IntegratedScientificResources.com
[dn.core]: https://www.bitbucket.org/davidhary/dn.core

[ISR's ONC RPC]: https://github.com/ATECoder/dn.onc.rpc
[IDE Repository]: https://www.bitbucket.org/davidhary/vs.ide
[external repositories]: ExternalReposCommits.csv

[ISR's XDR]: https://github.com/ATECoder/dn.xdr
[ISR's VXI-11]: https://github.com/ATECoder/dn.vxi11
[ISR's VXI-11 IEEE488]: https://github.com/ATECoder/dn.vxi11/src/vxi/ieee488
[XDR: External Data Representation Standard (May 2006)]: http://tools.ietf.org/html/rfc4506

[IVI Foundation]: https://www.ivifoundation.org

[XDR: External Data Representation Standard (May 2006)]: http://tools.ietf.org/html/rfc4506
[RPC: Remote Procedure Call Protocol Specification Version 2 (May 2009)]: http://tools.ietf.org/html/rfc5531
[Binding Protocols for ONC RPC Version 2 (August 1995)]: http://tools.ietf.org/html/rfc1833
[Sun RPC]: https://en.wikipedia.org/wiki/Sun_RPC
[VXI Bus Specification]: https://vxibus.org/specifications.html

[Jay Walter's SourceForge repository]: https://sourceforge.net/p/remoteteanet
[Wes Day's RemoteTea.Net]: https://github.com/wespday/RemoteTea.Net
[GB1.RemoteTea.Net]: https://github.com/galenbancroft/RemoteTea.Net
[org.acplt.oncrpc package]: https://people.eecs.berkeley.edu/~jonah/javadoc/org/acplt/oncrpc/package-summary.html
[Java ONC RPC]: https://github.com/remotetea/remotetea/tree/master/src/tests/org/acplt/oncrpc
[VXI11.CSharp]: https://github.com/Xanliang/VXI11.CSharp 


