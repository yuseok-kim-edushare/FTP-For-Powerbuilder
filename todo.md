0. use IL-Repack for merge dll into single dll file, for easier and avoid dependencies collision
1. implement connections.xml encrypt and decrypt with .net native api about AES256 GCM
    - if .net 48, use external call about windows Cryptographic Next Generation
    - if .net 8, use native aes gcm
2. update FTP Action using public api that paaaphrase or key for decrypt connections.xml
    - well safe key managing is needed at powerbuilder programm developer and application side
      just this dll project provide AES GCM based securing connection info
3. check interop test with powerbuilder
4. and implement other else my company usecase with legacy dll