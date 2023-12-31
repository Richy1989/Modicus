# Modicus
## Modular IoT Sensor Data Collector written in C# (nanoframework)

Modicus is an open-source software project designed for the ESP32 microcontroller, developed in C# using the NanoFramework.
It provides a highly modular and extensible framework for collecting data from various sensors and forwarding that data to an MQTT broker.
Whether you're working on IoT projects or sensor data collection applications, Modicus simplifies the process of connecting, collecting, and transmitting data.

## Features
- **Modular Design:** Modicus is designed with modularity in mind. You can easily add support for different sensors and communication protocols by creating modules, making it adaptable to a wide range of use cases.
- **Hotspot:** For first time setup, a hotspot is startet to make configuration as easy as possible
- **Support for Various Sensors:** Modicus supports a variety of sensors, including temperature sensors, humidity sensors, motion detectors, and more. You can easily integrate additional sensors by creating custom modules.
- **MQTT Integration:** Data collected from sensors is forwarded to an MQTT broker, allowing you to integrate Modicus with various IoT platforms or other services that support MQTT.
- **Flexible Configuration:** Modicus provides a configuration system that allows you to customize MQTT server settings, sensor parameters, and data transmission intervals to suit your specific requirements.
- **Webpage for Configuration:** Modicus provides a webserver for easy configuration of the whole system
- **Easy Deployment:** Deploy Modicus on ESP32 devices with NanoFramework, a lightweight .NET runtime for resource-constrained devices.


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
Download the newsest release. Execute the following in Power Shell. 

Note:
The last firmware version on which Modicus was tested is always attached in the release files. Since nanoframework is an active project there may be breaking changes and Modicus might not work if you use the latest firmware.

Flash Firmware on ESP32
```sh
nanoff --update --target <ESP Target> --serialport COM<X> --clrfile "<Path to nanoCLR.bin File>"
```

Flash Application on ESP32
```sh
nanoff --target <ESPTarget> --serialport <COMX> --deploy --image "<Path to Modicus BIN File>"
```

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
Payload JSON: {"On": "<true|false>"}
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
### Sensor On / Off / Add / Delete
Executes the given action on the selected sensor.
```sh
Topic: <ClientID>/cmd/SensorOnOff
Payload JSON:
{
	"Name": "<Name of sensor>",
	"Action": <0 = On | 1 = Off | 2 = AddOnly | 3 = Delete>
}
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

### Configure Wifi
Configures the wifi settings
```sh
Topic: <ClientID>/cmd/WifiControl
Payload JSON:
{
	"Ssid": "<Wifi SSID to connect>",
	"Password": "<Wifi passwort>",
	"IP": "<IP if static>",
	"NetworkMask": "<Network mask if static>",
	"DefaultGateway": "<Default Gateway if static>",
	"UseDHCP": <"TRUE | FALSE">,
	"Mode": <0 =  StartOnly | 1 = ConfigureAccessPoint | 2 = ConfigureWireless80211>
}
```

## Screen Shots
<img src="https://github.com/Richy1989/Modicus/blob/main/images/modicus_main.jpg" alt="Modicus Main Webpage" width="400"/>
<br />
<img src="https://github.com/Richy1989/Modicus/blob/main/images/modicus_sensor.jpg" alt="Modicus Add Sensor Page" width="400"/>

## License
This program is licensed under GPL-3.0-only
