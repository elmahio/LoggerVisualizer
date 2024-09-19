# <img src="icon.png" style="height: 1em; width: 1em;"> Logger Visualizer ![Build](https://github.com/elmahio/LoggerVisualizer/actions/workflows/dotnet.yml/badge.svg)

Logger Debug Visualizer for Visual Studio.

When debugging applications and an `ILogger` is available in the code you can view the logger configuration details in a more structured and visually pleasing form:

![Logger Visualizer](screenshot.png)

The extension shows all configured loggers with a set of default properties. For a range of known logging providers, the extension show a special UI. Like for `EventLog`:

![EventLog Logger Visualizer](screenshot2.png)

And elmah.io:

![ElmahIo Logger Visualizer](screenshot3.png)

Feel free to reach out or send a PR if you own a provider with additional content.

---

Sponsored by [elmah.io](https://elmah.io).