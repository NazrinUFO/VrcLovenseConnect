# VRCLovenseConnect
VRCLovenseConnect is a .NET implementation of the Lovense Connect API and Buttplug.io to synchronize **any connected toy** with a VRChat avatar through OSC messages.

## What does it do exactly?
With the power of **OSC** and **Avatar Dynamics**, your avatar sends a value between 0.0 and 1.0 to this program through UDP, according to the distance between a Contact Sender and the center of the Contact Receiver.

This value is converted and transferred to the Lovense Connect through HTTP, with a call to vibrate a connected toy.

It has been tested to be **accurate**, **lightweight** and **fast**. OSC is so fast indeed that I had to set a limit of processed messages to have less delay lol.

## Requirements
Lovense Connect for Android or iOS (also works on PC but requires the Lovense Bluetooth dongle).

## Quick Start
There are currently two protocols available for VRCLovenseConnect: "Lovense" and "Buttplug".

# Buttplug.io (NEW!)
Simply connect your toy to your PC through a cable or Bluetooth. Lovense toys should work with basic features in this protocol too.

You can test the connectivity of your toys with Intiface Desktop.

# Lovense Connect
Open the Lovense Connect app on your phone or PC, then select the green shield icon to reveal a URL.

In the config.json file next to the executable, copy this URL to the "address" field.

Then simply launch the program after VRChat is open.

## Avatar Setup
Your avatar requires a spherical Contact Receiver with "Proximity" mode. Set it up with any Contact Sender, whether it's a standard (hands, head...) or a custom one (sharing a name between your friends is a good idea).

This Contact Receiver has to be "Local Only" and generate a parameter with the same name as in the config.json file ("LovenseHaptics" by default). Reminder that parameters are case-sensitive.

Finally, add a float in your avatar's Expression Parameters with the same name, default to zero and no saving.

## config.json
oscPort: The port to listen OSC messages on. Default is 9001.

protocol: The protocol to use for toy controls (Lovense or Buttplug).

address: Only for Lovense protocol. The address provided by the Lovense Connect app on your phone (not Lovense Remote).

scanTime: Only for Buttplug protocol. The time to scan for a toy in seconds. Default is 5.

limit: Only for debugging. The number of received messages to skip for better performances. Default is 10.

parameter: The Avatar Parameter to synchronize with.

## What's Next?
I will implement more Lovense and Buttplug.io features in the future than just vibrations, with options to control several at a time or one by Contact Receiver for example.

## Documentation
VRChat OSC: https://docs.vrchat.com/docs/osc-overview

VRChat Contacts: https://docs.vrchat.com/v2022.1.2/docs/contacts

Intiface Desktop: https://intiface.com/desktop/

Buttplug.io C#: https://github.com/buttplugio/buttplug-rs-ffi/tree/master/csharp

LovenseConnectCSharp: https://github.com/MistressPlague/LovenseConnectCSharp
