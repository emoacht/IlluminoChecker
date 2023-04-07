# Illumino Checker

Visualize ambient light sensor output (lux) and corresponding screen brightness adjusted by adaptive brightness.

<img src="images/illumino-light.png" alt="Screenshot-light" width="500"> <img src="images/illumino-dark.png" alt="Screenshot-dark" width="500">

If you are not familier with adaptive brightness, take a look the overview by Microsoft.

- [Adaptive brightness](https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/sensors-adaptive-brightness)

It is good to know how it actually works under the hood.

In addition, this app records the sequence of illuminance and brightness to internal log which can be exported in CSV format. A log will be deleted after one week has passed to avoid taking up storage space.

## Requirements

 * Windows 10 or newer
 * Ambient Light Sensor (ALS)

## License

 - MIT License

## Libraries

 - [LiveCharts](https://v0.lvcharts.com/)
 - [Community.Toolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)

## Developer

 - emoacht (emotom[atmark]pobox.com)
