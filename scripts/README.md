# Linux release packaging

Publish the Linux x64 application from the repository root:

```sh
dotnet publish DynamicXtremeTool/DynamicXtremeTool.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o artifacts/linux-x64
```

Then run the packaging script on Linux (including WSL):

```sh
bash scripts/package-linux.sh artifacts/linux-x64 artifacts/DynamicXtreme-linux-x64.tar.gz
```

The script grants mode 755 to `dxt` and `Asar/asar` in a temporary copy,
creates a gzip-compressed tar archive, and verifies executable permissions after
extracting it to a Linux filesystem. It leaves the publication unchanged and
refuses to overwrite an existing archive. Shell scripts use LF line endings.

Distribute the `.tar.gz`, rather than a ZIP or a folder copied through Windows.
Extract with `tar -xzf DynamicXtreme-linux-x64.tar.gz` on Linux. Run the application
from the extracted `DynamicXtreme` directory, which must be writable.

When publishing from Windows, execute the packaging step from WSL in the same
repository directory. The archive carries Unix permissions even if the repository
is stored on a Windows filesystem. A full ROM installation remains a separate
release check.
