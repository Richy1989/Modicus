# Modicus
## A nanoframework Sensor Data Collector for MQTT written in C# (nanoframework)

### Built for the ESP32 Hardware
Welcome to the official GitHub page for our Modicus C# application designed to gather sensor data and seamlessly transmit it to an MQTT broker. This project empowers you to easily monitor and manage various sensors while ensuring smooth integration with MQTT-based systems.

## Features
- Read all kind of sensors with the help of the powerful nanoframework
- Webserver for easy configuration
- Wifi AP for setup
- Get all data via MQTT
- Control the application via MQTT

## Note
- This project is in alpha!
- The software is in active development and will grow
- Contribution is happily Welcome

## Supported sensors
This is a very new project. Supported devices will grow. The strucute of the project makes it very easy to add new sensors. 

Already Supported:
- BME280 Humidity / Pressure / Temperature Sensor
- CCS811 Gas Sensor

## Getting Started 
First of all follow [Getting Started Guide for managed code (C#)](https://docs.nanoframework.net/content/getting-started-guides/getting-started-managed.html).
Download the newest source, open with Visual Studio 2022 and deploy code. Use the Release Build for optimal performance.

###
Access Point: With the first startup a software access point will be started on the controller to reach the WebUI. 
Connect to the access point:
 - SSID: Modicus_&#60;MAC Address&#62;
 - Password: -
 - IP Adress: 192.168.4.1
 
Connect to the WebUI and configure the WiFi Settings. Then reboot the controller. It may happen that you need to reboot the controller several times. 
Note that GPIO 2 of the Controller is used as Signaling Service, if GPGIO pulses every 200ms you need to reboot again. 
Only if GPIP 2 is constant High the boot was successful. 

## MQTT Commands

### Turn MQTT Service ON / OFF
Turns the MQTT service on or off.
```sh
Topic: <ClientID>/cmd/MqttOnOff
Payload JSON: {"On": "true|false"}
```

### Set MQTT Client ID
Sets the client id for the MQTT service.
```sh
Topic: <ClientID>/cmd/MqttClientId
Payload JSON: {"ClientID": "<clientID>"}
```
### Set MQTT Send Interval 
Set the send interval for the MQTT service in seconds.
```sh
Topic: <ClientID>/cmd/MqttSendInterval
Payload JSON: {"Interval": <inverval>}
```

### Reboot system
Reboots the system after the given delay in seconds.
```sh
Topic: <ClientID>/cmd/Reboot
Payload JSON: {"Delay": <delay>}
```

### Set Measurement Interval for sensors
Set the measurement interval for the sensors in seconds.
```sh
Topic: <ClientID>/cmd/MeasurementInterval
Payload JSON: {"Interval": <inverval>}
```

### Create new I2C Connected Sensor
Creates and starts a new sensor connected to the I2C Bus and start it.
```sh
Topic: <ClientID>/cmd/CreateI2CSensor
Payload JSON:
{
	"SensorType": "<Supported Type>",
	"Name": "<Name>",
	"MeasurementInterval": <Measurement Interval>,
	"SclPin": <SCL Pin>,
	"SdaPin": <SDA Pin>,
	"BusID": <Bus ID>,
	"I2cBusSpeed": <0 = StandardMode | 1 = FastMode | 2 = FastModePlus>,
	"DeviceAddress": <I2C Address>
}
```

## Screen Shots
![Modicus Main Webpage](https://github.com/Richy1989/Modicus/blob/main/images/modicus_main.jpg?raw=true)

## License
This program is licensed under GPL-3.0-only
