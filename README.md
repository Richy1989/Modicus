# Modicus
## A nanoframework Sensor Data Collector for MQTT written in C# (nanoframework)
### Built for the ESP32 Hardware

Welcome to the official GitHub page for our Modicus C# application designed to gather sensor data and seamlessly transmit it to an MQTT broker. This project empowers you to easily monitor and manage various sensors while ensuring smooth integration with MQTT-based systems.

## Features

- Read all kinds of sensors with the help of the powerful nanoframework
- Webserver for easy configuration
- Wifi AP for setup
- Get all data via MQTT
- Control the application via MQTT

## Note
- This project is in alpha!
- This is a very new application, it can only read one sensor as of now (BME280). 
- The software is in active development and will grow
- Contribution is happily Welcome

## Getting Started 
First of all follow [Getting Started Guide for managed code (C#)](https://docs.nanoframework.net/content/getting-started-guides/getting-started-managed.html).
Download the newest release, open with Visual Studio 2022 and deploy code. Use the Release Build for optimal perfomance.

## MQTT Commands

### Turn MQTT Service ON / OFF
Turns the MQTT service on or off.
```sh
Topic: {ClientID}/cmd/MqttOnOff
Payload JSON: {"On"= "true|false"}
```

### Set MQTT Client ID
Sets the client id for the MQTT service.
```json
Topic: {ClientID}/cmd/MqttClientId
Payload JSON: {"ClientID"= "<clientID>"}
```
### Set MQTT Sent Interval 
Set the send interval for the MQTT service in seconds.
```json
Topic: {ClientID}/cmd/MqttSendInterval
Payload JSON: {"Interval"= <inverval>}
```

### Reboot system
Reboots the system after the given delay in seconds.
```json
Topic: {ClientID}/cmd/Reboot
Payload JSON: {"Interval"= <delay>}
```

### Set Measurement Interval for sensors
Set the measurement interval for the sensors in seconds.
```json
Topic: {ClientID}/cmd/MeasurementInterval
Payload JSON: {"Interval"= <inverval>}
```

## License
This program is licensed under GPL-3.0-only
