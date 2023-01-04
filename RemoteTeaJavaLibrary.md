# Remote Tea ONC/RPC Java Library

The whole release shebang is summarized in our [change log].

The most current information about the Remote Tea Java Library, as well as updates, can be found on the web at the [Remote Tea ONC/RPC Home Page]

## Licensing

The Remote Tea Java Library is licensed under the terms of the [GNU General Public License v2.0 (LGPL)].

### Rationale

while the LGPL does not restrict the use of my packages even in commercial applications, I am ensuring this way that modifications to the source code __must__ be given back to the community.

If you use my packages in commercial applications, you must either include the source code of the Remote Tea ONC/RPC Java Library or provide the source code to your customers on request. 

License details can be found in the [LGPL terms].

The individual file headers were moved to the [Remote Tea License] and [Remote Tea Header] files.

## Project Structure

The directory structure is as follows. Due to the transition to the Eclipse IDE and Ant not everything is currently in place and perfect shape.

* [Remote Tea ONC/RPC documentation] `javadoc/index.html`
* [Source code of the Remote Tea ONC/RPC Java Library] `src/`
    * `src/org/acplt/oncrpc` contains the source code of the various packages.
    * `src/org/acplt/oncrpc/apps` contains applications, in this case only the 
    `jrpcgen` rpc protocol compiler in the `jrpcgen/` subdirectory.
    * `src/tests` contains test code.
    * `src/tests/org/acplt/oncrpc/jrpcgen` contains an example of how to use the `jrpcgen` rpc protocol compiler. To generate source code from the x-file, run:
        ```
        java -jar jrpcgen.jar -p tests.org.acplt.oncrpc.jrpcgen -nobackup demo.x
        ```
    * `classes/` will receive the compiled Java byte code. After running the Ant `build.xml` makefile with target '`jar`', this directory will also contain two jar files called `classes/oncrpc.jar` and `classes/jrpcgen.jar`.

The "Remote Tea" ONC/RPC Java package was developed and tested using Eclipse on a JDK&nbsp;1.4.2 (Windows).

Those who like to compile the sources by themselves might want to use either the Eclipse `.project` file or the Ant makefile `build.xml` (this is not really functional yet), both are located in the root directory of this package.

## Who's to blame

The "Remote Tea" Java Package was written by [Harald Albrecht]  
(C)&nbsp;1999, 2003 Chair of Process Control Engineering,  
Aachen University of Technology,  
52064 Aachen, Germany  

[GNU General Public License v2.0 (LGPL)]: https://github.com/remotetea/remotetea/blob/master/COPYING.LIB
[LGPL terms]: https://github.com/remotetea/remotetea/blob/master/COPYING.LIB
[change log]: https://github.com/remotetea/remotetea/blob/master/changelog.html
[Remote Tea ONC/RPC Home Page]: "http://remotetea.sourceforge.net"
[Remote Tea ONC/RPC documentation]: https://github.com/remotetea/remotetea/blob/master/javadoc/index.html
[Source code of the Remote Tea ONC/RPC Java Library]: https://github.com/remotetea/remotetea/blob/master/src
[Harald Albrecht]: mailto:harald@plt.rwth-aachen.de

[Remote Tea Header]: src/rpc/RemoteTeaHeader  
[Remote Tea License]: RemoteTeaLicense

