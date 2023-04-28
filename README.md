### ONC RPC

Implementation of Sun's ONC/RPC Remote Procedure Protocol, including client and server functionality and some associated tools.

- [Description](#Description)
	- [History](#History)
	- [Standards](#Standards)
- [Source Code](#Source-Code)
  - [Repositories](#Repositories)
  - [Global Configuration Files](#Global-Configuration-Files)
- [Facilitated By](#FacilitatedBy)
- [Repository Owner](#Repository-Owner)
- [Authors](#Authors)
- [Acknowledgments](#Acknowledgments)
- [Open Source](#Open-Source)
- [Closed Software](#Closed-software)
- [Legal Notices](#Legal-Notices)

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

#### Source Code
Clone the repository along with its requisite repositories to their respective relative path.

##### Repositories
The repositories listed in [external repositories] are required:
* [IDE Repository] - IDE support files.
* [ISR's XDR] - eXternal Data Representation.
* [ISR's ONC RPC] - ONC/RPC.

```
git clone git@bitbucket.org:davidhary/vs.ide.git
git clone https://github.com/ATECoder/dn.xdr.git
git clone https://github.com/ATECoder/dn.onc.rpc.git
```

Clone the repositories into the following folders (parents of the .git folder):
```
%vslib%\core\ide
%dnlib%\iot\xdr
%dnlib%\iot\oncrpc
```
where %dnlib% and %vslib% are  the root folders of the .NET libraries, e.g., %my%\lib\vs 
and %my%\libraries\vs, respectively, and %my% is the root folder of the .NET solutions

##### Global Configuration Files
ISR libraries use a global editor configuration file and a global test run settings file. 
These files can be found in the [IDE Repository].

Restoring Editor Configuration:
```
xcopy /Y %my%\.editorconfig %my%\.editorconfig.bak
xcopy /Y %vslib%\core\ide\code\.editorconfig %my%\.editorconfig
```

Restoring Run Settings:
```
xcopy /Y %userprofile%\.runsettings %userprofile%\.runsettings.bak
xcopy /Y %vslib%\core\ide\code\.runsettings %userprofile%\.runsettings
```
where %userprofile% is the root user folder.

##### Packages
TBA

<a name="FacilitatedBy"></a>
#### Facilitated By
* [Visual Studio]
* [Atomineer Code Documentation]
* [EW Software Spell Checker]
* [Funduc Search and Replace]
* [IVI Foundation] - IVI Foundation VISA

#### Repository Owner
* [ATE Coder]

<a name="Authors"></a>
#### Authors
* [ATE Coder]  

<a name="Acknowledgments"></a>
#### Acknowledgments
* [Its all a remix] -- we are but a spec on the shoulders of giants  
* [John Simmons] - outlaw programmer  
* [Stack overflow] - Joel Spolsky  
* [.Net Foundation] - The .NET Foundation

<a name="Open-Source"></a>
#### Open source
Open source used by this software is described and licensed at the following sites:  

<a name="Closed-software"></a>
#### Closed software
Closed software used by this software are described and licensed on the following sites:  

<a name="Legal-Notices"></a>
#### Legal Notices

Integrated Scientific Resources, Inc., and any contributors grant you a license to the documentation and other content
in this repository under the [Creative Commons Attribution 4.0 International Public License](https://creativecommons.org/licenses/by/4.0/legalcode), see the [LICENSE](LICENSE) file, and grant you a license to any code in the repository under the [MIT License](https://opensource.org/licenses/MIT), see the [LICENSE-CODE](LICENSE-CODE) file.

Integrated Scientific Resources, Inc., and/or other Integrated Scientific Resources, Inc., products and services referenced in the documentation may be either trademarks or registered trademarks of Integrated Scientific Resources, Inc., in the United States and/or other countries. The licenses for this project do not grant you rights to use any Integrated Scientific Resources, Inc., names, logos, or trademarks.

Integrated Scientific Resources, Inc., and any contributors reserve all other rights, whether under their respective copyrights, patents, or trademarks, whether by implication, estoppel or otherwise.

[ISR's ONC RPC]: https://github.com/ATECoder/dn.onc.rpc
[IDE Repository]: https://www.bitbucket.org/davidhary/vs.ide
[external repositories]: ExternalReposCommits.csv

[ISR's XDR]: https://github.com/ATECoder/dn.xdr
[ISR's VXI-11]: https://github.com/ATECoder/dn.vxi11
[ISR's VXI-11 IEEE488]: https://github.com/ATECoder/dn.vxi11/src/vxi/ieee488
[XDR: External Data Representation Standard (May 2006)]: http://tools.ietf.org/html/rfc4506

[ATE Coder]: https://www.IntegratedScientificResources.com
[Its all a remix]: https://www.everythingisaremix.info
[John Simmons]: https://www.codeproject.com/script/Membership/View.aspx?mid=7741
[Stack overflow]: https://www.stackoveflow.com

[Visual Studio]: https://www.visualstudio.com/
[Jarte RTF Editor]: https://www.jarte.com/ 
[WiX Toolset]: https://www.wixtoolset.org/
[Atomineer Code Documentation]: https://www.atomineerutils.com/
[EW Software Spell Checker]: https://github.com/EWSoftware/VSSpellChecker/wiki/
[Code Converter]: https://github.com/icsharpcode/CodeConverter
[Funduc Search and Replace]: http://www.funduc.com/search_replace.htm

[.Net Foundation]: https://source.dot.net
[Microsoft .NET Framework]: https://dotnet.microsoft.com/download

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


