# VRCLovenseConnect
VRCLovenseConnect is an updated implementation of LovenseConnectCSharp to synchronize any Lovense toy with a VRChat avatar through OSC messages.

## What does it do exactly?
With the power of **OSC** and **Avatar Dynamics**, your avatar sends a value between 0.0 and 1.0 to this program through UDP, according to the distance between a Contact Sender and the center of the Contact Receiver.

This value is converted and transferred to the Lovense Connect through HTTP, with a call to vibrate a connected toy.

It has been tested to be **accurate**, **lightweight** and **fast** (OSC is so fast indeed that I had to set a limit of received messages to be more performant lol).

## Requirements
Lovense Connect for Android or iOS (also works on PC but requires the Lovense Bluetooth dongle).

## Quick Start
Open the Lovense Connect app on your phone or PC, then select the green shield icon to reveal a URL.

In the config.json file next to the executable, copy this URL to the "address" field.

Then simply launch the program after VRChat is open.

## Avatar Setup
Your avatar requires a spherical Contact Receiver with "Proximity" mode. Set it up with any Contact Sender, whether it's a standard (hands, head...) or a custom one.

This Contact Receiver has to be "Local Only" and generate a parameter with the same name as in the config.json file ("LovenseHaptics" by default). Reminder that parameters are case-sensitive.

Finally, add a float in your avatar's Expression Parameters with the same name, default to zero and no saving.

## config.json
oscPort: Port number of the program to listen on OSC messages (default is 9001).
address: URL to connect to the Lovense Connect app.
limit: For debugging purpose only. Sets a number of received messages to skip for better performances (default is 10).
parameter: The name of the parameter sent by the avatar (default is LovenseHaptics).

## Documentation
https://docs.vrchat.com/docs/osc-overview

https://docs.vrchat.com/v2022.1.2/docs/contacts

https://github.com/MistressPlague/LovenseConnectCSharp
