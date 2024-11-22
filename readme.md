# AMNet Test Server
This can be used to test the card switching application without access to a supported game.
No documentation or support is provided except for the contents below.

## Usage
- On the [releases](https://github.com/aspriddell/amnet-test-server/releases) page, locate and download the latest version for your platform.
- Run the executable without any arguments. The server will be available at `http://<your-ip-address>:6070`, which should be entered in the switcher app.

### macOS
> [!WARNING]
> macOS will block the executable from running due to gatekeeper.
> If running the app, and it is killed, go to Privacy & Security settings and allow the app to run.

```shell
chmod +x ./amnet-test-server-osx-arm64 # if permission is denied, this will fix it (applies to linux as well)
./amnet-test-server-osx-arm64 # run the server
```

## License
This project is licensed under the MIT License - see the [license.md](license.md) file for details.
