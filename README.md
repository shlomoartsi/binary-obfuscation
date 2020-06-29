# binary-obfuscation
proof of concept for binary obfuscation
This project uses a technique developed by C.Linn and S.Debray described in the article: https://dl.acm.org/citation.cfm?id=948149
The project demonstrates inserting junk bytes into selected places on assembly code. 
The junk bytes would cause parsing errors by standard tools as 'dumpbin' or 'objdump' which parse executable binaries to assembly code.
This project is implemented with C#, and works on x86 Windows assemblies.
The project is a proof of concept and doesn't obfuscate most of the tested executables, so the community is encouraged to improve it!
The project uses 2 open source projects: PeNet and SharpDisasm.
