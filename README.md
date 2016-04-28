# Win10IoT-Thermo-IoTSuite-Demo
Demo of Windows 10 IoT Core running on Raspberry PI connecting to Azure IoT Suite.

This project has been created for demo purposes by sascha@corti.com (@TechPreacher).

This project is free to use for any purposes. Please respect the license terms that apply to 3rd party libraries or code used in this project (see *External Libraries / Code* section)

##Configuring the solution

First create a Microsoft Azure Iot Suite (http://azureiotsuite.com/) project of type *Remote monitoring*.

In the Azure Portal (http://portal.azure.com), open the Resource Group that was created, containing your IoT Suite solution and expand the *IoT Hub* node. Note the *Hostname* which has the format \[solutionname\].azure-devices.net.

In the *IoT Hub* configuration, expand the *Shared Access Policies* node and open the *iothubowner* entry.
Note the *Primary key*.

Find all the fields labled \[replace\] in the solution and add the appropriate values as noted above.

Use your IoT Suite's main landing page's (https://\[solutionname\].azurewebsites.net/Dashboard/Index) "Add device" function to create a new device. Note it's *Name* and *Authentication Key 1* which are needed by the main project *Win10IoT Thermo*.

##Projects in the Solution

###Read Device To Cloud Messages
This project can be used to read all messages that go into the *IoT Hub* of your IoT Suite solution.

###Create Device To Cloud Messages
This porject simulates a sensor that sends random temperature and humidity values to your IoT Suite solution.

###Sensors.Dht
This project contains the driver used by the Windows 10 IoT UWP app to talk to the DHT11 single wire temperature/humidity sensor.  

###Windows10 IoT Thermo (Universal Windows)
This is the main UWP app that runs on the Raspberry PI, reads temperature and humidity values and submits them to the IoT Suite solution.

##Hardware schema
The schema for building the sensor assembly can be found in the Fritzing files attached.

**Win10IoT Thermo Diagram Cobbler.fzz** contains the hardware schema if connected directly to the Raspberry PI's outputs.

**Win10IoT Thermo Diagram.fzz** contains the hardware schema using a cobbler.

The *Fritzing* app can be obtained here: http://fritzing.org/download/

##External Libraries / Code

This solution uses the C++ DHT11 senosr driver created by Daniel Porrey which can be found at:
https://microsoft.hackster.io/en-US/porrey/dht11-dht22-temperature-sensor-077790?ref=search&ref_id=dht11&offset=0

##Known issues
none.
