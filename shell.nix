with import <nixpkgs> { };

pkgs.mkShell {
  name = "dotnet-env";
  packages = [
    dotnetCorePackages.sdk_7_0
    dotnetCorePackages.runtime_7_0
    chromium
    icu
    openssl
  ];
  shellHook = ''
    export LD_LIBRARY_PATH="$LD_LIBRARY_PATH:${
      with pkgs;
      lib.makeLibraryPath [ icu openssl ]
    }"
    export CHROME_PATH="${chromium}/bin/chromium"
  '';
}
